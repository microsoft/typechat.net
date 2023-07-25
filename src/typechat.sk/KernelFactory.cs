// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.SemanticKernel;

public static class KernelFactory
{
    public static TypeChatJsonTranslator<T> JsonTranslator<T>(SchemaText schema, ModelInfo model, OpenAIConfig config)
    {
        ArgumentNullException.ThrowIfNull(model, nameof(model));

        // Create kernel
        KernelBuilder kb = new KernelBuilder();
        kb.WithChatModel(model.Name, config);
        IKernel kernel = kb.Build();
        // And Json translator
        return kernel.JsonTranslator<T>(schema, model);
    }

    public static TypeChatJsonTranslator<T> JsonTranslator<T>(ModelInfo model, OpenAIConfig config)
    {
        ArgumentNullException.ThrowIfNull(model, nameof(model));

        // Create kernel
        KernelBuilder kb = new KernelBuilder();
        kb.WithChatModel(model.Name, config);
        IKernel kernel = kb.Build();
        // And Json translator
        return kernel.JsonTranslator<T>(model);
    }

    public static TypeChatJsonTranslator<T> JsonTranslator<T>(OpenAIConfig config)
    {
        ArgumentNullException.ThrowIfNull(config, nameof(config));

        return JsonTranslator<T>(config.Model, config);
    }

    public static TypeChatJsonTranslator<T> JsonTranslator<T>(this IKernel kernel, TypeChat.SchemaText schema, ModelInfo model)
    {
        TypeChatJsonTranslator<T> typechat = new TypeChatJsonTranslator<T>(
            kernel.CompletionService(model),
            new JsonSerializerTypeValidator<T>(schema)
        );
        return typechat;
    }

    public static TypeChatJsonTranslator<T> JsonTranslator<T>(this IKernel kernel, ModelInfo model)
    {
        TypeSchema schema = TypescriptExporter.GenerateSchema(typeof(T));
        TypeChatJsonTranslator<T> typechat = new TypeChatJsonTranslator<T>(
            kernel.CompletionService(model),
            new JsonSerializerTypeValidator<T>(schema)
        );
        return typechat;
    }
}
