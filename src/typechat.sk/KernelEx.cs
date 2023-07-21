// Copyright (c) Microsoft. All rights reserved.
using Microsoft.SemanticKernel.AI.TextCompletion;

namespace Microsoft.TypeChat.SemanticKernel;

public static class KernelEx
{
    public static KernelBuilder WithChatModels(this KernelBuilder builder, OpenAIConfig config, params string[] modelNames)
    {
        foreach(string modelName in modelNames)
        {
            builder.WithChatModel(modelName, config);
        }
        return builder;
    }

    public static KernelBuilder WithChatModel(this KernelBuilder builder, string modelName, OpenAIConfig config)
    {
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

    public static CompletionService CompletionService(this IKernel kernel, ModelInfo model)
    {
        return new CompletionService(kernel.GetService<ITextCompletion>(model.Name), model);
    }
}
