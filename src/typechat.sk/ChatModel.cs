// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public class ChatModel : IChatModel
{
    IChatCompletion _service;
    ModelInfo _model;

    public ChatModel(OpenAIConfig config, ModelInfo? model = null)
    {
        ArgumentNullException.ThrowIfNull(config, nameof(config));

        model ??= config.Model;
        IKernel kernel = config.CreateKernel();
        _service = kernel.GetService<IChatCompletion>(model.Name);
        _model = model;
    }

    public ChatModel(IChatCompletion service, ModelInfo model)
    {
        ArgumentNullException.ThrowIfNull(service, nameof(service));
        _service = service;
        _model = model;
    }

    public async Task<string> GetResponseAsync(IChatMessage message, IEnumerable<IChatMessage>? context = null, RequestSettings? settings = null, CancellationToken cancelToken = default)
    {
        ChatHistory history = ToHistory(message, context);
        ChatRequestSettings? requestSettings = ToRequestSettings(settings);
        string textResponse = await _service.GenerateMessageAsync(history, requestSettings, cancelToken).ConfigureAwait(false);
        return textResponse;
    }

    ChatHistory ToHistory(IChatMessage message, IEnumerable<IChatMessage>? contextMessages)
    {
        ChatHistory history = new ChatHistory();
        if (contextMessages != null)
        {
            history.Append(contextMessages);
        }
        history.Append(message);
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
