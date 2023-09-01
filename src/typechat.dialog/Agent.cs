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
        history ??= new MessageList();
        _history = history;
    }

    public Prompt Preamble => _preamble;
    public IMessageStream History => _history;
    public RequestSettings RequestSettings { get; set; }
    public bool RetainResponse { get; set; } = false;

    public int MaxPromptLength
    {
        get => _builder.MaxLength;
        set => _builder.MaxLength = value;
    }

    public async Task<T> ProcessRequest(string request, CancellationToken cancelToken = default)
    {
        Prompt context = BuildContext();
        context.Push(request);
        T response = await _translator.TranslateAsync(context, null, RequestSettings, cancelToken);
        _history.Append(new Message(request));
        if (RetainResponse)
        {
            _history.Append(new Message(response));
        }
        return response;
    }

    Prompt BuildContext()
    {
        _builder.Clear();
        if (_builder.AddRange(_preamble))
        {
            _builder.AddHistory(_history);
        }
        return _builder.Prompt;
    }
}
