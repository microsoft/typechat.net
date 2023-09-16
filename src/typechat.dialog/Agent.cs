// Copyright (c) Microsoft. All rights reserved.

using Microsoft.TypeChat.Schema;

namespace Microsoft.TypeChat.Dialog;

/// <summary>
/// Experimental:
/// A multi-turn message passing Agent that returns strongly typed responses
/// The Agent automatically maintains interaction history. For every request, it automatically
/// retrieves pertinent context from this history and includes it in the request. You can control
/// the size of each request by setting the MaxRequestPromptLength property
/// Message
/// </summary>
public class Agent<T>
{
    JsonTranslator<T> _translator;
    IMessageStream _history;
    Prompt _instructions;
    PromptBuilder _builder;
    int _maxPromptLength;

    /// <summary>
    /// Create a new Agent that uses the given language model
    /// By default, the agent will use an in-memory transient message history.
    /// </summary>
    /// <param name="model">language model the agent uses</param>
    public Agent(ILanguageModel model)
        : this(new JsonTranslator<T>(model))
    {
    }
    /// <summary>
    /// Create a new Agent that uses the given language model
    /// By default, the agent will use an in-memory transient message history.
    /// </summary>
    /// <param name="translator">Translator to use</param>
    /// <param name="history">Customize an object to use to capture this agent's interaction history</param>
    /// <exception cref="ArgumentNullException">If translator is null</exception>
    public Agent(JsonTranslator<T> translator, IMessageStream? history = null)
    {
        if (translator == null)
        {
            throw new ArgumentNullException(nameof(translator));
        }
        _translator = translator;
        _instructions = new Prompt();
        _builder = new PromptBuilder(translator.Model.ModelInfo.MaxCharCount / 2);
        _maxPromptLength = _builder.MaxLength;
        _history = history ?? new MessageList();
    }

    /// <summary>
    /// Instructions to the model: these let you customize how the Agent will behave
    /// </summary>
    public Prompt Instructions => _instructions;
    /// <summary>
    /// The translator being used by the Agent to go to strongly typed messages
    /// </summary>
    public JsonTranslator<T> Translator => _translator;
    /// <summary>
    /// This Agent's interaction history. The Agent automatically uses this history to inject
    /// context with every request.
    /// </summary>
    public IMessageStream History => _history;
    /// <summary>
    /// Transform raw model responses into messages for the message history
    /// Lets you customize what you write into history: you may not always want to store the raw T in history
    /// especially if T is a complex Json
    /// </summary>
    public Func<T, Message?> TransformResponseForHistory { get; set; }
    /// <summary>
    /// The maximum number of characters to put in a request prompt. The prompt contains
    /// - the user's request
    /// - any automatically collected context, from history 
    /// </summary>
    public int MaxRequestPromptLength
    {
        get => _maxPromptLength;
        set => _maxPromptLength = value;
    }

    /// <summary>
    /// Translate the user request to T
    /// </summary>
    /// <param name="request">request</param>
    /// <param name="cancelToken">optional cancellation token</param>
    /// <returns>value of type T</returns>
    public async Task<T> TranslateAsync(string request, CancellationToken cancelToken = default)
    {
        Prompt context = BuildContext(request);
        T response = await _translator.TranslateAsync(request, context, null, cancelToken).ConfigureAwait(false);

        // Save user message in the interaction history
        _history.Append(request);

        Message? responseMessage = (TransformResponseForHistory != null) ?
                                  TransformResponseForHistory(response) :
                                  Message.FromAssistant(response);
        if (responseMessage != null)
        {
            _history.Append(responseMessage);
        }
        return response;
    }

    Prompt BuildContext(string request)
    {
        int requestLength = request.Length;
        _builder.Clear();
        _builder.MaxLength = (_maxPromptLength - requestLength);
        // Add any preamble       
        _builder.AddRange(_instructions);
        if (_history.GetCount() > 0)
        {
            _builder.Add(PromptSection.Instruction("IMPORTANT CONTEXT for the user request:"));
            _builder.AddHistory(_history.GetContext(request));
        }
        return _builder.Prompt;
    }
}
