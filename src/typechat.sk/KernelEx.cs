// Copyright (c) Microsoft. All rights reserved.

using Azure.Core;
using Microsoft.SemanticKernel.TextGeneration;

namespace Microsoft.TypeChat;

public static class KernelEx
{
    /// <summary>
    /// Create a Semantic Kernel using the given OpenAI configuration
    /// Automatically sets up retries
    /// </summary>
    /// <param name="config">OpenAI configuration</param>
    /// <returns>A kernel object</returns>
    public static Kernel CreateKernel(this OpenAIConfig config)
    {
        // Create kernel
        IKernelBuilder kb = Kernel.CreateBuilder();
        kb.WithChatModel(config.Model, config);

        Kernel kernel = kb.Build();
        return kernel;
    }

    /// <summary>
    /// Configure the kernel to use the given chat models
    /// </summary>
    /// <param name="builder">kernel builder</param>
    /// <param name="config">OpenAI configuration</param>
    /// <param name="modelNames">names of chat models</param>
    /// <returns>kernel builder</returns>
    public static IKernelBuilder WithChatModels(this IKernelBuilder builder, OpenAIConfig config, params string[] modelNames)
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
    public static IKernelBuilder WithChatModel(this IKernelBuilder builder, string modelName, OpenAIConfig config)
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
            if (config.HasTokenProvider)
            {
                builder = builder.AddAzureOpenAIChatCompletion(modelName, config.Endpoint, (TokenCredential)config.ApiTokenProvider.GetCredential(), modelName, modelName, client);
            }
            else
            {
                builder = builder.AddAzureOpenAIChatCompletion(modelName, config.Endpoint, config.ApiKey, modelName, modelName, client);
            }
        }
        else
        {
            builder = builder.AddOpenAIChatCompletion(modelName, config.ApiKey, config.Organization, modelName, client);
        }
        return builder;
    }

    /// <summary>
    /// Configure a kernel with an embedding model
    /// </summary>
    /// <param name="builder">builder</param>
    /// <param name="modelName">model to use</param>
    /// <param name="config">OpenAI configuration</param>
    /// <returns>builder</returns>
    public static IKernelBuilder WithEmbeddingModel(this IKernelBuilder builder, string modelName, OpenAIConfig config)
    {
        if (config.Azure)
        {
            if (config.HasTokenProvider)
            {
                builder = builder.AddAzureOpenAITextEmbeddingGeneration(modelName, config.Endpoint, (TokenCredential)config.ApiTokenProvider.GetCredential(), modelName);
            }
            else
            {
                builder = builder.AddAzureOpenAITextEmbeddingGeneration(modelName, config.Endpoint, config.ApiKey, modelName);
            }
        }
        else
        {
            builder = builder.AddOpenAITextEmbeddingGeneration(modelName, config.ApiKey, config.Organization, modelName);
        }
        return builder;
    }

    /// <summary>
    /// Create a new language model using the given Kernel
    /// </summary>
    /// <param name="kernel">semantic kernel object</param>
    /// <param name="model">information about the model to use</param>
    /// <returns>LanguageModel object</returns>
    public static ChatLanguageModel ChatLanguageModel(this Kernel kernel, ModelInfo model)
    {
        ArgumentVerify.ThrowIfNull(model, nameof(model));

        return new ChatLanguageModel(kernel.GetRequiredService<IChatCompletionService>(model.Name), model);
    }

    /// <summary>
    /// Create a new Text completion model using the given kernel
    /// </summary>
    /// <param name="kernel">semantic kernel object</param>
    /// <param name="model">information about the model to use</param>
    /// <returns>TextCompletionModel</returns>
    public static TextCompletionModel TextCompletionModel(this Kernel kernel, ModelInfo model)
    {
        ArgumentVerify.ThrowIfNull(model, nameof(model));

        return new TextCompletionModel(kernel.GetRequiredService<ITextGenerationService>(model.Name), model);
    }

    public static async Task<string> GenerateMessageAsync(this IChatCompletionService service, ChatHistory history, OpenAIPromptExecutionSettings settings, CancellationToken cancelToken)
    {
        var content = await service.GetChatMessageContentAsync(history, settings).ConfigureAwait(false);
        return content.Content;
    }
}
