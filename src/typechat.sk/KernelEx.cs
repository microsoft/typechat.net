// Copyright (c) Microsoft. All rights reserved.
using Microsoft.SemanticKernel.AI.TextCompletion;
using Microsoft.SemanticKernel.Reliability;

namespace Microsoft.TypeChat;

public static class KernelEx
{
    public static IKernel CreateKernel(this OpenAIConfig config, string? modelName = null)
    {
        modelName ??= config.Model;
        ArgumentException.ThrowIfNullOrEmpty(modelName, nameof(modelName));

        // Create kernel
        KernelBuilder kb = new KernelBuilder();
        kb.WithChatModel(modelName, config)
          .WithRetry(config);

        IKernel kernel = kb.Build();
        return kernel;
    }

    public static KernelBuilder WithChatModels(this KernelBuilder builder, OpenAIConfig config, params string[] modelNames)
    {
        ArgumentNullException.ThrowIfNull(config, nameof(config));

        foreach (string modelName in modelNames)
        {
            builder.WithChatModel(modelName, config);
        }
        return builder;
    }

    public static KernelBuilder WithChatModel(this KernelBuilder builder, string modelName, OpenAIConfig config)
    {
        ArgumentNullException.ThrowIfNull(config, nameof(config));

        HttpClient client = null;
        if (config.TimeoutMs > 0)
        {
            client = new HttpClient();
            client.Timeout = TimeSpan.FromMilliseconds(config.TimeoutMs);
        }
        if (config.Azure)
        {
            builder = builder.WithAzureChatCompletionService(modelName, config.Endpoint, config.ApiKey, true, modelName, false, client);
        }
        else
        {
            builder = builder.WithOpenAIChatCompletionService(modelName, config.ApiKey, config.Organization, modelName, true, false, client);
        }
        return builder;
    }

    public static KernelBuilder WithRetry(this KernelBuilder builder, OpenAIConfig config)
    {
        TimeSpan retryPause = TimeSpan.FromMilliseconds(config.MaxPauseMs);
        HttpRetryConfig retryConfig = new HttpRetryConfig
        {
            MaxRetryDelay = retryPause,
            MaxRetryCount = config.MaxRetries,
            UseExponentialBackoff = false
        };
        return builder.WithRetryHandlerFactory(new DefaultHttpRetryHandlerFactory(retryConfig));
    }

    public static CompletionService CompletionService(this IKernel kernel, ModelInfo model)
    {
        ArgumentNullException.ThrowIfNull(model, nameof(model));

        return new CompletionService(kernel.GetService<ITextCompletion>(model.Name), model);
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

    internal static bool IsGlobal(this FunctionView fview)
    {
        return ("_GLOBAL_FUNCTIONS_" == fview.SkillName);
    }

    internal static bool HasDescription(this ParameterView param)
    {
        return !string.IsNullOrEmpty(param.Description);
    }

    internal static PluginFunctionName ToPlugin(this FunctionView fview)
    {
        // Temporary hack to make pretty printing possible
        if (fview.IsGlobal())
        {
            return new PluginFunctionName(fview.Name);
        }
        return new PluginFunctionName(fview.SkillName, fview.Name);
    }
}
