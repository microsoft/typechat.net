// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Dialog;

/// <summary>
/// A simple, multi-turn agent
/// </summary>
public class Agent<T>
{
    JsonTranslator<T> _translator;
    IMessageStream _history;
    Prompt _preamble;
    PromptBuilder _builder;
    int _maxPromptLength;

    public Agent(ILanguageModel model, IVocabCollection? vocabs = null)
        : this(new JsonTranslator<T>(model, new TypeValidator<T>(vocabs)))
    {
    }

    public Agent(JsonTranslator<T> translator, IMessageStream? history = null)
    {
        ArgumentNullException.ThrowIfNull(translator, nameof(translator));
        _translator = translator;
        _preamble = new Prompt();
        _builder = new PromptBuilder(translator.Model.ModelInfo.MaxCharCount / 2);
        _maxPromptLength = _builder.MaxLength;
        _history = history ?? new MessageList();
    }

    public JsonTranslator<T> Translator => _translator;
    public Prompt Preamble => _preamble;
    public RequestSettings RequestSettings { get; set; }
    public IMessageStream InteractionHistory => _history;

    /// <summary>
    /// Save user requests
    /// </summary>
    public bool SaveRequest { get; set; } = true;
    /// <summary>
    /// Place JSON responses in history
    /// </summary>
    public bool SaveResponse { get; set; } = true;

    public int MaxPromptLength
    {
        get => _maxPromptLength;
        set => _maxPromptLength = value;
    }

    public async Task<T> TranslateAsync(string request, CancellationToken cancelToken = default)
    {
        Prompt prompt = BuildPrompt(request);
        T response = await _translator.TranslateAsync(prompt, _preamble, RequestSettings, cancelToken).ConfigureAwait(false);
        if (SaveRequest)
        {
            _history.Append(request);
        }
        if (SaveResponse)
        {
            _history.Append(Message.FromAssistant(response));
        }
        return response;
    }

    Prompt BuildPrompt(string request)
    {
        int preambleLength = _preamble.GetLength();
        int availableContextLength = (_maxPromptLength - (preambleLength + request.Length));
        if (availableContextLength <= 0)
        {
            return request;
        }

        _builder.Clear();
        _builder.MaxLength = availableContextLength;
        _builder.AddContext(_history.Nearest(request));
        _builder.MaxLength += request.Length;
        _builder.Add(request);

        return _builder.Prompt;
    }
}
