// Copyright (c) Microsoft. All rights reserved.
using Microsoft.SemanticKernel.AI.TextCompletion;
using Microsoft.SemanticKernel.Reliability;

namespace Microsoft.TypeChat.SemanticKernel;

public static class KernelEx
{
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
}
