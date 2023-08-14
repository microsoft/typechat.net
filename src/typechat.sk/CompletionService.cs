// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public class CompletionService : ILanguageModel
{
    ITextCompletion _service;
    ModelInfo _model;

    public CompletionService(OpenAIConfig config, ModelInfo? model = null)
    {
        ArgumentNullException.ThrowIfNull(config, nameof(config));

        model ??= config.Model;
        // Create kernel
        KernelBuilder kb = new KernelBuilder();
        kb.WithChatModel(config.Model, config)
          .WithRetry(config);

        IKernel kernel = kb.Build();
        _service = kernel.GetService<ITextCompletion>(model.Name);
        _model = model;
    }

    public CompletionService(ITextCompletion service, ModelInfo model)
    {
        ArgumentNullException.ThrowIfNull(service, nameof(service));
        _service = service;
        _model = model;
    }

    public ModelInfo Model => _model;

    public async Task<string> CompleteAsync(string prompt, RequestSettings? settings, CancellationToken cancelToken)
    {
        CompleteRequestSettings? requestSettings = ToRequestSettings(settings);
        string response = await _service.CompleteAsync(prompt, requestSettings, cancelToken).ConfigureAwait(false);
        return response;
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
