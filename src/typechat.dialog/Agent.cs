// Copyright (c) Microsoft. All rights reserved.

using Microsoft.TypeChat.Schema;

namespace Microsoft.TypeChat.Dialog;

/// <summary>
/// Experimental:
/// A multi-turn message passing Agent that uses a JsonTranslator to create strongly typed responses to user requests. 
/// 
/// The Agent automatically maintains interaction history. For every request, the agent
/// retrieves pertinent context from this history and includes it in the request. You can control
/// the size of each request by setting the MaxRequestPromptLength property.
/// 
/// </summary>
public class Agent<T> : IAgent
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
    /// Get response for the given request
    /// </summary>
    /// <param name="message">request message</param>
    /// <param name="cancelToken">optional cancel token</param>
    /// <returns>Response message</returns>
    public Task<Message> GetResponseMessageAsync(Message message, CancellationToken cancelToken = default)
    {
        return GetResponseMessageAsync(message, null, cancelToken);
    }

    /// <summary>
    /// Get response for the given request
    /// </summary>
    /// <param name="request">request message</param>
    /// <param name="createMessageForHistory">Customize how a response is written into history</param>
    /// <param name="cancelToken">optional cancellation token</param>
    /// <returns>response message</returns>
    public async Task<Message> GetResponseMessageAsync(
        Message request,
        Func<T, Message?> createMessageForHistory,
        CancellationToken cancelToken = default)
    {
        ArgumentVerify.ThrowIfNull(request, nameof(request));

        string requestText = request.GetText();
        Prompt context = BuildContext(requestText);

        T response = await _translator.TranslateAsync(requestText, context, null, cancelToken).ConfigureAwait(false);
        Message responseMessage = Message.FromAssistant(response);

        // Save user message in the interaction history
        _history.Append(request);
        // Create a message to save in history
        Message? historyMessage = (createMessageForHistory != null) ?
                                  createMessageForHistory(response) :
                                  responseMessage;
        if (historyMessage != null)
        {
            _history.Append(historyMessage);
        }

        return responseMessage;
    }

    /// <summary>
    /// Get a response to the given message
    /// </summary>
    /// <param name="request">request</param>
    /// <param name="createMessageForHistory">Customize how a response is written into history</param>
    /// <param name="cancelToken">optional cancel token</param>
    /// <returns>Response object</returns>
    public async Task<T> GetResponseAsync(
        Message request,
        Func<T, Message?>? createMessageForHistory = null,
        CancellationToken cancelToken = default)
    {
        Message response = await GetResponseMessageAsync(request, createMessageForHistory, cancelToken).ConfigureAwait(false);
        return response.GetBody<T>();
    }

    Prompt BuildContext(string request)
    {
        int requestLength = request.Length;
        _builder.Clear();
        _builder.MaxLength = (_maxPromptLength - requestLength);
        // Add any preamble       
        _builder.AddRange(_instructions);
        //
        // If we have history, find parts of it that are contextually relevant and add
        //
        if (_history.GetCount() > 0)
        {
            _builder.Add(PromptSection.Instruction("IMPORTANT CONTEXT for the user request:"));

            IEnumerable<IPromptSection> context = _history.GetContext(request);
            if (context != null)
            {
                _builder.AddHistory(context);
            }
        }
        return _builder.Prompt;
    }
}
