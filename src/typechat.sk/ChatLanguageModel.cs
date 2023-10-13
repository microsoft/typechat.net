// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// A LanguageModel that uses IChatCompletion AI models
/// </summary>
public class ChatLanguageModel : ILanguageModel
{
    IChatCompletion _service;

    /// <summary>
    /// Create a new language model from the OpenAI config
    /// By default, uses model in config.Model
    /// </summary>
    /// <param name="config"></param>
    /// <param name="model">information about the model to create</param>
    public ChatLanguageModel(OpenAIConfig config, ModelInfo? model = null)
    {
        ArgumentVerify.ThrowIfNull(config, nameof(config));
        config.Validate();

        model ??= config.Model;
        IKernel kernel = config.CreateKernel();
        _service = kernel.GetService<IChatCompletion>(model.Name);
        ModelInfo = model;
    }

    /// <summary>
    /// Create a new language model using the supplied IChatCompletion service
    /// </summary>
    /// <param name="service"></param>
    /// <param name="model"></param>
    public ChatLanguageModel(IChatCompletion service, ModelInfo model)
    {
        ArgumentVerify.ThrowIfNull(service, nameof(service));
        _service = service;
        ModelInfo = model;
    }

    /// <summary>
    /// Information about the model
    /// </summary>
    public ModelInfo ModelInfo { get; }

    /// <summary>
    /// Return a completion for the prompt
    /// </summary>
    /// <param name="prompt"></param>
    /// <param name="settings"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<string> CompleteAsync(Prompt prompt, TranslationSettings? settings = null, CancellationToken cancellationToken = default)
    {
        ChatHistory history = ToHistory(prompt);
        ChatRequestSettings? requestSettings = ToRequestSettings(settings);
        return _service.GenerateMessageAsync(history, requestSettings, cancellationToken);
    }

    ChatHistory ToHistory(Prompt prompt)
    {
        ChatHistory history = new ChatHistory();
        history.Append(prompt);
        return history;
    }

    ChatRequestSettings? ToRequestSettings(TranslationSettings? settings)
    {
        if (settings is null)
        {
            return null;
        }

        var requestSettings = new ChatRequestSettings();
        if (settings.Temperature >= 0)
        {
            requestSettings.Temperature = settings.Temperature;
        }

        if (settings.MaxTokens > 0)
        {
            requestSettings.MaxTokens = settings.MaxTokens;
        }

        return requestSettings;
    }
}
