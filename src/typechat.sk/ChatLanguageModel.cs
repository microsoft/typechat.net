// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// A LanguageModel that uses IChatCompletionService AI models
/// </summary>
public class ChatLanguageModel : ILanguageModel
{
    private readonly IChatCompletionService _service;
    private readonly ModelInfo _model;

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
        Kernel kernel = config.CreateKernel();
        _service = kernel.GetRequiredService<IChatCompletionService>(model.Name);
        _model = model;
    }

    /// <summary>
    /// Create a new language model using the supplied IChatCompletionService service
    /// </summary>
    /// <param name="service"></param>
    /// <param name="model"></param>
    public ChatLanguageModel(IChatCompletionService service, ModelInfo model)
    {
        ArgumentVerify.ThrowIfNull(service, nameof(service));
        _service = service;
        _model = model;
    }

    /// <summary>
    /// Information about the model
    /// </summary>
    public ModelInfo ModelInfo => _model;

    /// <summary>
    /// Return a completion for the prompt
    /// </summary>
    /// <param name="prompt"></param>
    /// <param name="settings"></param>
    /// <param name="cancelToken"></param>
    /// <returns></returns>
    public async Task<string> CompleteAsync(Prompt prompt, TranslationSettings? settings = null, CancellationToken cancelToken = default)
    {
        ChatHistory history = ToHistory(prompt);
        OpenAIPromptExecutionSettings? requestSettings = ToRequestSettings(settings);
        string textResponse = await _service.GenerateMessageAsync(history, requestSettings, cancelToken).ConfigureAwait(false);
        return textResponse;
    }

    private ChatHistory ToHistory(Prompt prompt)
    {
        ChatHistory history = new ChatHistory();
        history.Append(prompt);
        return history;
    }

    private OpenAIPromptExecutionSettings? ToRequestSettings(TranslationSettings? settings)
    {
        if (settings is null)
        {
            return null;
        }
        var requestSettings = new OpenAIPromptExecutionSettings();
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
