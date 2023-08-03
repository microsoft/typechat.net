// Copyright (c) Microsoft. All rights reserved.
using Microsoft.SemanticKernel.AI.TextCompletion;
using Microsoft.SemanticKernel.Reliability;

namespace Microsoft.TypeChat.SemanticKernel;

public static class KernelEx
{
    public static KernelBuilder WithChatModels(this KernelBuilder builder, OpenAIConfig config, params string[] modelNames)
    {
        ArgumentNullException.ThrowIfNull(config, nameof(config));

        foreach(string modelName in modelNames)
        {
            builder.WithChatModel(modelName, config);
        }
        return builder;
    }

    public static KernelBuilder WithChatModel(this KernelBuilder builder, string modelName, OpenAIConfig config)
    {
        ArgumentNullException.ThrowIfNull(config, nameof(config));

        if (config.Azure)
        {
            builder = builder.WithAzureChatCompletionService(modelName, config.Endpoint, config.ApiKey, true, modelName);
        }
        else
        {
            builder = builder.WithOpenAIChatCompletionService(modelName, config.ApiKey, config.Organization, modelName, true);
        }
        return builder;
    }

    public static KernelBuilder WithRetry(this KernelBuilder builder, int maxRetries, TimeSpan? retryPauseMs = null)
    {
        retryPauseMs ??= TimeSpan.FromMilliseconds(100);
        HttpRetryConfig config = new HttpRetryConfig
        {
            MinRetryDelay = retryPauseMs.Value,
            MaxRetryDelay = retryPauseMs.Value,
            MaxRetryCount = maxRetries,
            UseExponentialBackoff = false
        };
        return builder.WithRetryHandlerFactory(new DefaultHttpRetryHandlerFactory(config));
    }

    public static CompletionService CompletionService(this IKernel kernel, ModelInfo model)
    {
        ArgumentNullException.ThrowIfNull(model, nameof(model));

        return new CompletionService(kernel.GetService<ITextCompletion>(model.Name), model);
    }
}
