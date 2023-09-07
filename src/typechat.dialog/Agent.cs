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

    public Agent(JsonTranslator<T> translator, IMessageStream? history = null)
    {
        ArgumentNullException.ThrowIfNull(translator, nameof(translator));
        _translator = translator;
        _preamble = new Prompt();
        _builder = new PromptBuilder(translator.Model.ModelInfo.MaxCharCount / 2);
        _history = history ?? new MessageList();
    }

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
    /// <summary>
    /// Flatten history into a single prompt before sending to the model
    /// </summary>
    public bool InlineContext { get; set; } = true;

    public int MaxPromptLength
    {
        get => _builder.MaxLength;
        set => _builder.MaxLength = value;
    }

    public async Task<T> TranslateAsync(string request, CancellationToken cancelToken = default)
    {
        Prompt context = BuildContext(request);
        T response;
        if (InlineContext)
        {
            context.Push(request);
            response = await _translator.TranslateAsync(context.ToString(true), null, RequestSettings, cancelToken).ConfigureAwait(false);
        }
        else
        {
            response = await _translator.TranslateAsync(request, context, RequestSettings, cancelToken).ConfigureAwait(false);
        }
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

    Prompt BuildContext(string request)
    {
        _builder.Clear();
        if (_preamble.Count > 0)
        {
            _builder.AddRange(_preamble);
        }
        _builder.AddContext(_history.Nearest(request));
        return _builder.Prompt;
    }
}
