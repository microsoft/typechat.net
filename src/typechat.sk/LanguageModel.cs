// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public class LanguageModel : ILanguageModel
{
    IChatCompletion _service;
    ModelInfo _model;

    public LanguageModel(OpenAIConfig config, ModelInfo? model = null)
    {
        ArgumentNullException.ThrowIfNull(config, nameof(config));
        config.Validate();

        model ??= config.Model;
        IKernel kernel = config.CreateKernel();
        _service = kernel.GetService<IChatCompletion>(model.Name);
        _model = model;
    }

    public LanguageModel(IChatCompletion service, ModelInfo model)
    {
        ArgumentNullException.ThrowIfNull(service, nameof(service));
        _service = service;
        _model = model;
    }

    public ModelInfo ModelInfo => _model;

    public async Task<string> CompleteAsync(Prompt prompt, RequestSettings? settings = null, CancellationToken cancelToken = default)
    {
        ChatHistory history = ToHistory(prompt);
        ChatRequestSettings? requestSettings = ToRequestSettings(settings);
        string textResponse = await _service.GenerateMessageAsync(history, requestSettings, cancelToken).ConfigureAwait(false);
        return textResponse;
    }

    ChatHistory ToHistory(Prompt prompt)
    {
        ChatHistory history = new ChatHistory();
        history.Append(prompt);
        return history;
    }

    ChatRequestSettings? ToRequestSettings(RequestSettings? settings)
    {
        if (settings == null)
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
