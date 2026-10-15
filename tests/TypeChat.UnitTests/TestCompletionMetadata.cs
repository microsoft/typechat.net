// Copyright (c) Microsoft. All rights reserved.

using System.Net.Http;
using System.Text.Json;

namespace Microsoft.TypeChat.Tests;

public class TestCompletionMetadata
{
    public class Example
    {
        public int Value { get; set; }
    }

    private sealed class LegacyModel : ILanguageModel
    {
        public ModelInfo ModelInfo => "legacy";
        public Prompt LastPrompt { get; private set; }
        public TranslationSettings LastSettings { get; private set; }
        public CancellationToken LastToken { get; private set; }
        public int Calls { get; private set; }

        public Task<string> CompleteAsync(Prompt prompt, TranslationSettings? settings, CancellationToken cancelToken)
        {
            Calls++;
            LastPrompt = prompt;
            LastSettings = settings;
            LastToken = cancelToken;
            return Task.FromResult("{\"Value\":42}");
        }
    }

    private sealed class MetadataModel : ILanguageModelWithMetadata
    {
        private readonly Queue<object> _responses;

        public MetadataModel(params object[] responses) => _responses = new Queue<object>(responses);

        public ModelInfo ModelInfo => "metadata";
        public int LegacyCalls { get; private set; }
        public int MetadataCalls { get; private set; }
        public CancellationToken LastToken { get; private set; }

        public Task<string> CompleteAsync(Prompt prompt, TranslationSettings? settings, CancellationToken cancelToken)
        {
            LegacyCalls++;
            return Task.FromResult("{\"Value\":7}");
        }

        public Task<LanguageModelResponse> CompleteWithMetadataAsync(Prompt prompt, TranslationSettings? settings, CancellationToken cancelToken)
        {
            MetadataCalls++;
            LastToken = cancelToken;
            return _responses.Dequeue() switch
            {
                Exception error => Task.FromException<LanguageModelResponse>(error),
                Task<LanguageModelResponse> pending => pending,
                LanguageModelResponse response => Task.FromResult(response),
                _ => throw new InvalidOperationException("Unexpected test completion")
            };
        }
    }

    private sealed class HookTranslator : JsonTranslator<Example>
    {
        public HookTranslator(ILanguageModel model) : base(model, Schema()) { }

        public int LegacyCalls { get; private set; }
        public int MetadataCalls { get; private set; }

        protected override Task<string> GetResponseAsync(Prompt prompt, TranslationSettings settings, CancellationToken cancelToken)
        {
            LegacyCalls++;
            return Task.FromResult("{\"Value\":21}");
        }

        protected override Task<LanguageModelResponse> GetResponseWithMetadataAsync(Prompt prompt, TranslationSettings settings, CancellationToken cancelToken)
        {
            MetadataCalls++;
            return Task.FromResult(new LanguageModelResponse("{\"Value\":84}", new CompletionInfo { Model = "custom" }));
        }
    }

    private static SchemaText Schema() =>
        new SchemaText("export interface Example { Value: number; }", SchemaText.Languages.Typescript);

    private static JsonTranslator<Example> Translator(ILanguageModel model, int repairs = 1) =>
        new JsonTranslator<Example>(model, Schema()) { MaxRepairAttempts = repairs };

    [Fact]
    public async Task TextOnlyModelsSupportTheOptionalMetadataApi()
    {
        var model = new LegacyModel();
        Prompt prompt = "request";
        var settings = new TranslationSettings { Temperature = 0.5 };
        using var cancellation = new CancellationTokenSource();

        LanguageModelResponse response = await model.CompleteWithMetadataAsync(prompt, settings, cancellation.Token);

        Assert.Equal("{\"Value\":42}", response.Text);
        Assert.Null(response.Info);
        Assert.Same(prompt, model.LastPrompt);
        Assert.Same(settings, model.LastSettings);
        Assert.Equal(cancellation.Token, model.LastToken);
        Assert.Equal(1, model.Calls);
        Task<string> originalContract = model.CompleteAsync(prompt, settings, cancellation.Token);
        Assert.Equal(response.Text, await originalContract);
    }

    [Fact]
    public async Task OptionalCapabilityIsDispatchedThroughTheOriginalInterface()
    {
        var info = new CompletionInfo { Model = "reported-model" };
        var expected = new LanguageModelResponse("completion", info);
        var model = new MetadataModel(expected);
        ILanguageModel originalInterface = model;

        var actual = await originalInterface.CompleteWithMetadataAsync("request");

        Assert.Same(expected, actual);
        Assert.Equal(1, model.MetadataCalls);
        Assert.Equal(0, model.LegacyCalls);
    }

    [Fact]
    public async Task OriginalTranslationStillUsesTheOriginalModelMethod()
    {
        var model = new MetadataModel();

        Example result = await Translator(model).TranslateAsync("request");

        Assert.Equal(7, result.Value);
        Assert.Equal(1, model.LegacyCalls);
        Assert.Equal(0, model.MetadataCalls);
    }

    [Fact]
    public async Task ExistingAndMetadataTranslationHooksRemainSeparate()
    {
        var model = new LegacyModel();
        var translator = new HookTranslator(model);

        Assert.Equal(21, (await translator.TranslateAsync("request")).Value);
        var result = await translator.TranslateToResultAsync("request");

        Assert.Equal(84, result.Value.Value);
        Assert.Equal("custom", result.Info.Model);
        Assert.Equal(1, translator.LegacyCalls);
        Assert.Equal(1, translator.MetadataCalls);
        Assert.Equal(0, model.Calls);
    }

    [Fact]
    public async Task ResultTranslationSupportsTextOnlyModelsWithoutInventingUsage()
    {
        var result = await Translator(new LegacyModel()).TranslateToResultAsync("request");

        Assert.True(result.Success);
        Assert.Equal(42, result.Value.Value);
        Assert.Equal(0, result.Info.RepairAttempts);
        Assert.Null(result.Info.Usage);
        Assert.Null(result.Info.Model);
        Assert.Null(result.Failure);
    }

    [Fact]
    public async Task SuccessfulRepairRetainsFinalMetadataAndDiagnosticEvents()
    {
        var first = new CompletionInfo { Usage = new TokenUsage(10, 5, 15) };
        var last = new CompletionInfo { Model = "last", Usage = new TokenUsage(20, 5, 25) };
        var model = new MetadataModel(
            new LanguageModelResponse("{\"Value\":\"invalid\"}", first),
            new LanguageModelResponse("{\"Value\":42}", last));
        var translator = Translator(model);
        var sent = new List<Prompt>();
        var received = new List<string>();
        var repairs = new List<string>();
        translator.SendingPrompt += sent.Add;
        translator.CompletionReceived += received.Add;
        translator.AttemptingRepair += repairs.Add;

        var result = await translator.TranslateToResultAsync("request");

        Assert.True(result.Success);
        Assert.Equal(42, result.Value.Value);
        Assert.Equal(1, result.Info.RepairAttempts);
        Assert.Equal(25, result.Info.Usage.TotalTokens);
        Assert.Equal("last", result.Info.Model);
        Assert.NotSame(last, result.Info);
        Assert.Null(last.RepairAttempts);
        Assert.Null(first.RepairAttempts);
        Assert.Null(result.Failure);
        Assert.Equal(2, sent.Count);
        Assert.Equal(2, received.Count);
        Assert.Single(repairs);
    }

    [Fact]
    public async Task FailedRepairRetainsCandidateContextAndFinalCompletionMetadata()
    {
        var last = new CompletionInfo { Model = "last", Usage = new TokenUsage(20, 5, 25) };
        var model = new MetadataModel(
            new LanguageModelResponse("{\"Value\":\"invalid\"}"),
            new LanguageModelResponse("not JSON", last));

        var result = await Translator(model).TranslateToResultAsync("request");

        Assert.False(result.Success);
        Assert.Equal("invalid", Assert.IsType<JsonElement>(result.Failure.Value).GetProperty("Value").GetString());
        Assert.Equal("not JSON", result.Failure.ResponseText);
        Assert.Equal("last", result.Info.Model);
        Assert.Equal(1, result.Info.RepairAttempts);
        Assert.Equal(25, result.Info.Usage.TotalTokens);
        Assert.Null(last.RepairAttempts);
    }

    [Theory]
    [InlineData("not JSON")]
    [InlineData("{\"Value\":")]
    [InlineData("} invalid {")]
    [InlineData("{")]
    public async Task InvalidModelOutputReturnsValidationFailure(string text)
    {
        var model = new MetadataModel(new LanguageModelResponse(text));

        var result = await Translator(model, 0).TranslateToResultAsync("request");

        Assert.False(result.Success);
        Assert.Equal(text, result.Failure.ResponseText);
        Assert.False(result.Failure.HasValue);
        Assert.Equal(0, result.Info.RepairAttempts);
        Assert.Equal(1, model.MetadataCalls);
    }

    [Theory]
    [InlineData("http", false)]
    [InlineData("http", true)]
    [InlineData("cancel", false)]
    [InlineData("cancel", true)]
    [InlineData("programming", false)]
    [InlineData("programming", true)]
    public async Task ResultApiPreservesExceptionIdentityAndCancellation(string kind, bool repairing)
    {
        using var cancellation = new CancellationTokenSource();
        Exception expected = kind switch
        {
            "http" => new HttpRequestException("transport failure"),
            "cancel" => new OperationCanceledException(cancellation.Token),
            _ => new InvalidOperationException("model bug")
        };
        var model = repairing ?
            new MetadataModel(new LanguageModelResponse("{\"Value\":\"invalid\"}"), expected) :
            new MetadataModel(expected);

        Exception actual = await Record.ExceptionAsync(() =>
            Translator(model).TranslateToResultAsync("request", cancellation.Token));

        Assert.Same(expected, actual);
        Assert.Equal(cancellation.Token, model.LastToken);
        Assert.Equal(repairing && kind == "http", actual.Data.Contains(FailureInfo.ExceptionDataKey));
        if (actual is OperationCanceledException canceled)
        {
            Assert.Equal(cancellation.Token, canceled.CancellationToken);
        }
    }

    [Fact]
    public async Task ConcurrentTranslationsDoNotShareCompletionMetadata()
    {
        var first = new TaskCompletionSource<LanguageModelResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        var second = new TaskCompletionSource<LanguageModelResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        var translator = Translator(new MetadataModel(first.Task, second.Task));

        Task<Result<Example>> firstCall = translator.TranslateToResultAsync("first");
        Task<Result<Example>> secondCall = translator.TranslateToResultAsync("second");
        second.SetResult(new LanguageModelResponse("{\"Value\":2}", new CompletionInfo { Model = "second" }));
        Result<Example> secondResult = await secondCall;
        Assert.Equal("second", secondResult.Info.Model);
        first.SetResult(new LanguageModelResponse("{\"Value\":1}", new CompletionInfo { Model = "first" }));

        Result<Example> firstResult = await firstCall;
        Assert.Equal("first", firstResult.Info.Model);
        Assert.Equal(1, firstResult.Value.Value);
        Assert.Equal(2, secondResult.Value.Value);
    }

    [Fact]
    public void ResultConversionPreservesBothKindsOfMetadata()
    {
        var info = new CompletionInfo { Model = "test" };
        var source = Result<object?>.Error("invalid").WithFailure(new FailureInfo { Value = "rejected" });
        source.Info = info;

        var result = new Result<Example>(source);

        Assert.False(result.Success);
        Assert.Null(result.Value);
        Assert.Same(info, result.Info);
        Assert.Same(source.Failure, result.Failure);
    }

    [Fact]
    public void AbsentCompletionMetadataDoesNotChangeSerializedResults()
    {
        var result = new Result<int>(42);
        using JsonDocument json = JsonDocument.Parse(JsonSerializer.Serialize(result));

        Assert.False(json.RootElement.TryGetProperty("Info", out _));
        Assert.False(json.RootElement.TryGetProperty("Failure", out _));
        Assert.Equal(42, json.RootElement.GetProperty("Value").GetInt32());
    }
}
