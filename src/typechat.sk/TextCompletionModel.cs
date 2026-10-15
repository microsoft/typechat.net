// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel.TextGeneration;

namespace Microsoft.TypeChat;

/// <summary>
/// A language model that can work models that support the ITextGenerationService interface
/// </summary>
public class TextCompletionModel : ILanguageModel
{
    private readonly Kernel _kernel;
    private readonly ITextGenerationService _service;
    private readonly ModelInfo _model;

    public TextCompletionModel(OpenAIConfig config, ModelInfo? model = null)
    {
        ArgumentVerify.ThrowIfNull(config, nameof(config));

        config.Validate();

        model ??= config.Model;
        _kernel = config.CreateKernel();
        _service = _kernel.GetRequiredService<ITextGenerationService>(model.Name);
        _model = model;
    }

    public TextCompletionModel(ITextGenerationService service, ModelInfo model)
    {
        ArgumentVerify.ThrowIfNull(service, nameof(service));
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

    public async Task<string> CompleteAsync(Prompt prompt, TranslationSettings? settings = null, CancellationToken cancelToken = default)
    {
        OpenAIPromptExecutionSettings? requestSettings = ToRequestSettings(settings);
        string request = prompt.ToString(IncludeSectionSource);
        var response = await _service.GetTextContentsAsync(request, requestSettings, _kernel, cancelToken).ConfigureAwait(false);
        return response.Count > 0 ? response[0].Text : string.Empty;
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
