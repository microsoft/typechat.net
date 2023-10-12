// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel.Reliability.Basic;

namespace Microsoft.TypeChat;

public static class KernelExtensions
{
    /// <summary>
    /// Create a Semantic Kernel using the given OpenAI configuration
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

    /// <summary>
    /// Configure the kernel to use the given chat models
    /// </summary>
    /// <param name="builder">kernel builder</param>
    /// <param name="config">OpenAI configuration</param>
    /// <param name="modelNames">names of chat models</param>
    /// <returns>kernel builder</returns>
    public static KernelBuilder WithChatModels(this KernelBuilder builder, OpenAIConfig config, params string[] modelNames)
    {
        ArgumentVerify.ThrowIfNull(config, nameof(config));

        foreach (string modelName in modelNames)
        {
            builder.WithChatModel(modelName, config);
        }
        return builder;
    }

    /// <summary>
    /// Configure the kernel to use the given chat model
    /// Automatically configures the builder with retries and other settings
    /// </summary>
    /// <param name="builder">kernel builder</param>
    /// <param name="modelName">name of the model to use</param>
    /// <param name="config">OpenAI configuration for the model</param>
    /// <returns>builder</returns>
    public static KernelBuilder WithChatModel(this KernelBuilder builder, string modelName, OpenAIConfig config)
    {
        ArgumentVerify.ThrowIfNull(config, nameof(config));

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

    /// <summary>
    /// Configure the kernel with retry settings defined in the OpenAI config
    /// </summary>
    /// <param name="builder">builder</param>
    /// <param name="config">OpenAI configuration</param>
    /// <returns>builder</returns>
    public static KernelBuilder WithRetry(this KernelBuilder builder, OpenAIConfig config)
    {
        TimeSpan retryPause = TimeSpan.FromMilliseconds(config.MaxPauseMs);
        BasicRetryConfig retryConfig = new BasicRetryConfig
        {
            MaxRetryDelay = retryPause,
            MaxRetryCount = config.MaxRetries,
            UseExponentialBackoff = false
        };
        return builder.WithRetryBasic(retryConfig);
    }

    /// <summary>
    /// Configure a kernel with an embedding model
    /// </summary>
    /// <param name="builder">builder</param>
    /// <param name="modelName">model to use</param>
    /// <param name="config">OpenAI configuration</param>
    /// <returns>builder</returns>
    public static KernelBuilder WithEmbeddingModel(this KernelBuilder builder, string modelName, OpenAIConfig config)
    {
        if (config.Azure)
        {
            builder = builder.WithAzureTextEmbeddingGenerationService(modelName, config.Endpoint, config.ApiKey, modelName);
        }
        else
        {
            builder = builder.WithOpenAITextEmbeddingGenerationService(modelName, config.ApiKey, config.Organization, modelName);
        }
        return builder;
    }

    /// <summary>
    /// Create a new language model using the given Kernel
    /// </summary>
    /// <param name="kernel">semantic kernel object</param>
    /// <param name="model">information about the model to use</param>
    /// <returns>LanguageModel object</returns>
    public static ChatLanguageModel ChatLanguageModel(this IKernel kernel, ModelInfo model)
    {
        ArgumentVerify.ThrowIfNull(model, nameof(model));

        return new ChatLanguageModel(kernel.GetService<IChatCompletion>(model.Name), model);
    }

    /// <summary>
    /// Create a new Text completion model using the given kernel
    /// </summary>
    /// <param name="kernel">semantic kernel object</param>
    /// <param name="model">information about the model to use</param>
    /// <returns>TextCompletionModel</returns>
    public static TextCompletionModel TextCompletionModel(this IKernel kernel, ModelInfo model)
    {
        ArgumentVerify.ThrowIfNull(model, nameof(model));

        return new TextCompletionModel(kernel.GetService<ITextCompletion>(model.Name), model);
    }
}
