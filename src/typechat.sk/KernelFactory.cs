// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.SemanticKernel;

public static class KernelFactory
{
    public static ILanguageModel CreateLanguageModel(OpenAIConfig config)
    {
        IKernel kernel = CreateKernel(config);
        return kernel.CompletionService(config.Model);
    }

    public static IKernel CreateKernel(OpenAIConfig config, string? modelName = null)
    {
        modelName ??= config.Model;
        ArgumentException.ThrowIfNullOrEmpty(modelName, nameof(modelName));

        // Create kernel
        KernelBuilder kb = new KernelBuilder();
        kb.WithChatModel(modelName, config)
          .WithRetry(config.MaxRetries, config.MaxPauseMs);

        IKernel kernel = kb.Build();
        return kernel;
    }

    public static JsonTranslator<T> JsonTranslator<T>(OpenAIConfig config)
    {
        ArgumentNullException.ThrowIfNull(config, nameof(config));
        return JsonTranslator<T>(config.Model, config);
    }

    public static JsonTranslator<T> JsonTranslator<T>(SchemaText schema, ModelInfo model, OpenAIConfig config)
    {
        ArgumentNullException.ThrowIfNull(model, nameof(model));
        IKernel kernel = CreateKernel(config, model.Name);
        return kernel.JsonTranslator<T>(schema, model);
    }

    public static JsonTranslator<T> JsonTranslator<T>(ModelInfo model, OpenAIConfig config)
    {
        ArgumentNullException.ThrowIfNull(model, nameof(model));
        IKernel kernel = CreateKernel(config, model.Name);
        // And Json translator
        return kernel.JsonTranslator<T>(model);
    }

    public static JsonTranslator<T> JsonTranslator<T>(this IKernel kernel, TypeChat.SchemaText schema, ModelInfo model)
    {
        JsonTranslator<T> translator = new JsonTranslator<T>(kernel.CompletionService(model), schema);
        return translator;
    }

    public static JsonTranslator<T> JsonTranslator<T>(this IKernel kernel, ModelInfo model, IVocabCollection? vocabs = null)
    {
        TypescriptSchema schema = TypescriptExporter.GenerateSchema(typeof(T), vocabs);
        JsonTranslator<T> translator = new JsonTranslator<T>(
            kernel.CompletionService(model),
            new TypeValidator<T>(schema)
        );
        return translator;
    }
}
