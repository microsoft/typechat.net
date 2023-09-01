﻿// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel.AI.TextCompletion;
using Microsoft.SemanticKernel.Reliability;

namespace Microsoft.TypeChat;

public static class KernelEx
{
    /// <summary>
    /// Create a Semantic Kernel using the given Open AI configuration
    /// Automatically sets up retries
    /// </summary>
    /// <param name="config">OpenAI configuration</param>
    /// <returns>A kernel object</returns>
    public static IKernel CreateKernel(this OpenAIConfig config)
    {
        // Create kernel
        KernelBuilder kb = new KernelBuilder();
        kb.WithChatModel(config.Model, config)
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

    public static LanguageModel LanguageModel(this IKernel kernel, ModelInfo model)
    {
        ArgumentNullException.ThrowIfNull(model, nameof(model));

        return new LanguageModel(kernel.GetService<IChatCompletion>(model.Name), model);
    }

    public static TextCompletionModel TextCompletionModel(this IKernel kernel, ModelInfo model)
    {
        ArgumentNullException.ThrowIfNull(model, nameof(model));

        return new TextCompletionModel(kernel.GetService<ITextCompletion>(model.Name), model);
    }
}
