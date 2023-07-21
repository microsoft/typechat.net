// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.SemanticKernel;

public class CompletionService : ITypeChatLanguageModel
{
    ITextCompletion _service;
    ModelInfo _model;

    public CompletionService(ITextCompletion service, ModelInfo model)
    {
        ArgumentNullException.ThrowIfNull(service, nameof(service));
        _service = service;
        _model = model;
    }

    public ModelInfo Model => _model;

    public Task<string> CompleteAsync(string prompt, RequestSettings? settings, CancellationToken cancelToken)
    {
        CompleteRequestSettings? requestSettings = ToRequestSettings(settings);
        return _service.CompleteAsync(prompt, requestSettings, cancelToken);
    }

    CompleteRequestSettings? ToRequestSettings(RequestSettings? settings)
    {
        if (settings == null)
        {
            return null;
        }
        var requestSettings = new CompleteRequestSettings();
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
