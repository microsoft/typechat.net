// Copyright (c) Microsoft. All rights reserved.

using System.Net.Http;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Microsoft.TypeChat.Tests;

public class TestLegacyBinaryCompatibility
{
    [Fact]
    public async Task ConsumersCompiledAgainstTheOriginalContractsStillLoadAndRun()
    {
        Assembly currentAssembly = typeof(ILanguageModel).Assembly;
        string version = currentAssembly.GetName().Version.ToString();
        // This reference contract deliberately has no metadata API. It is never loaded at runtime.
        string originalContract = $$"""
            using System.Threading;
            using System.Threading.Tasks;
            [assembly: System.Reflection.AssemblyVersion("{{version}}")]
            namespace Microsoft.TypeChat
            {
                public class Prompt { }
                public class TranslationSettings { }
                public class ModelInfo { }
                public interface IJsonTypeValidator<T> { }
                public interface IJsonTranslatorPrompts { }
                public interface ILanguageModel
                {
                    ModelInfo ModelInfo { get; }
                    Task<string> CompleteAsync(Prompt prompt, TranslationSettings settings, CancellationToken cancelToken);
                }
                public class LanguageModel
                {
                    public Task<string> CompleteAsync(Prompt prompt, TranslationSettings settings, CancellationToken cancelToken) => null;
                }
                public class JsonTranslator<T>
                {
                    public JsonTranslator(ILanguageModel model, IJsonTypeValidator<T> validator, IJsonTranslatorPrompts prompts) { }
                    protected virtual Task<string> GetResponseAsync(Prompt prompt, TranslationSettings settings, CancellationToken cancelToken) => null;
                }
            }
            """;
        const string consumer = """
            using System.Collections.Generic;
            using System.Threading;
            using System.Threading.Tasks;
            using Microsoft.TypeChat;
            public sealed class OriginalModel : ILanguageModel
            {
                public ModelInfo ModelInfo => null;
                public Task<string> CompleteAsync(Prompt prompt, TranslationSettings settings, CancellationToken cancelToken) =>
                    Task.FromResult("original model");
            }
            public sealed class OriginalTranslator : JsonTranslator<Dictionary<string, int>>
            {
                public OriginalTranslator(ILanguageModel model, IJsonTypeValidator<Dictionary<string, int>> validator)
                    : base(model, validator, null) { }
                protected override Task<string> GetResponseAsync(Prompt prompt, TranslationSettings settings, CancellationToken cancelToken) =>
                    Task.FromResult("{\"Value\":42}");
            }
            public static class OriginalCaller
            {
                public static Task<string> Complete(LanguageModel model, Prompt prompt, TranslationSettings settings, CancellationToken cancelToken) =>
                    model.CompleteAsync(prompt, settings, cancelToken);
            }
            """;
        MetadataReference[] framework =
        [
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Collections").Location)
        ];
        byte[] contract = Compile(currentAssembly.GetName().Name, originalContract, framework);
        byte[] plugin = Compile("TypeChat.OriginalConsumer", consumer, framework.Append(MetadataReference.CreateFromImage(contract)));
        var loadContext = new AssemblyLoadContext("TypeChat original consumer", isCollectible: true);
        loadContext.Resolving += (_, name) => name.Name == currentAssembly.GetName().Name ? currentAssembly : null;
        try
        {
            using var stream = new MemoryStream(plugin);
            Assembly assembly = loadContext.LoadFromStream(stream);
            var model = (ILanguageModel)Activator.CreateInstance(assembly.GetType("OriginalModel", throwOnError: true));
            Task<string> text = model.CompleteAsync("request", null, default);
            Assert.Equal("original model", await text);
            Assert.Null((await model.CompleteWithMetadataAsync("request")).Info);

            var validator = new JsonSerializerTypeValidator<Dictionary<string, int>>(
                new SchemaText("export interface Example { Value: number; }", SchemaText.Languages.Typescript));
            var translator = (JsonTranslator<Dictionary<string, int>>)Activator.CreateInstance(
                assembly.GetType("OriginalTranslator", throwOnError: true), model, validator);
            Assert.Equal(42, (await translator.TranslateAsync("request"))["Value"]);

            var originalCall = assembly.GetType("OriginalCaller", throwOnError: true).GetMethod("Complete")
                .CreateDelegate<Func<LanguageModel, Prompt, TranslationSettings, CancellationToken, Task<string>>>();
            var config = new OpenAIConfig
            {
                Azure = false, ApiKey = "test", Model = "test", Endpoint = "https://api.example/v1/chat/completions"
            };
            using var httpModel = new LanguageModel(config, client: new HttpClient(
                new MockHttpHandler("{\"choices\":[{\"message\":{\"content\":\"original caller\"}}]}")));
            Assert.Equal("original caller", await originalCall(httpModel, "request", null, default));
        }
        finally
        {
            loadContext.Unload();
        }
    }

    private static byte[] Compile(string name, string source, IEnumerable<MetadataReference> references)
    {
        var compilation = CSharpCompilation.Create(name, new[] { CSharpSyntaxTree.ParseText(source) },
            references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        using var stream = new MemoryStream();
        var result = compilation.Emit(stream);
        Assert.True(result.Success, string.Join(Environment.NewLine, result.Diagnostics));
        return stream.ToArray();
    }
}
