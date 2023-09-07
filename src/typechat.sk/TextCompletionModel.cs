// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public class TextCompletionModel : ILanguageModel
{
    ITextCompletion _service;
    ModelInfo _model;

    public TextCompletionModel(OpenAIConfig config, ModelInfo? model = null)
    {
        ArgumentNullException.ThrowIfNull(config, nameof(config));

        model ??= config.Model;
        IKernel kernel = config.CreateKernel();
        _service = kernel.GetService<ITextCompletion>(model.Name);
        _model = model;
    }

    public TextCompletionModel(ITextCompletion service, ModelInfo model)
    {
        ArgumentNullException.ThrowIfNull(service, nameof(service));
        _service = service;
        _model = model;
    }

    public ModelInfo ModelInfo => _model;
    /// <summary>
    /// If true, will include the source of the prompt section
    /// user:\n Hello
    /// assistant:\n Goodbye
    /// </summary>
    public bool IncludeSectionSource { get; set; } = true;

    public async Task<string> CompleteAsync(Prompt prompt, RequestSettings? settings = null, CancellationToken cancelToken = default)
    {
        CompleteRequestSettings? requestSettings = ToRequestSettings(settings);
        string request = prompt.ToString(IncludeSectionSource);
        string response = await _service.CompleteAsync(request, requestSettings, cancelToken).ConfigureAwait(false);
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
