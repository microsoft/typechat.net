// Copyright (c) Microsoft. All rights reserved.

using Microsoft.TypeChat.Schema;

namespace Microsoft.TypeChat.Dialog;

/// <summary>
/// An Agent that uses a JsonTranslator to create strongly typed responses to user requests. 
/// The agent also optionally retrieves context for each interaction from the supplied context provider. 
/// </summary>
public class Agent<T> : IAgent
{
    JsonTranslator<T> _translator;
    IContextProvider? _contextProvider;
    Prompt _instructions;
    PromptBuilder _builder;
    int _maxPromptLength;

    /// <summary>
    /// Create a new Agent that uses the given language model
    /// The agent .
    /// </summary>
    /// <param name="model">language model the agent uses</param>
    /// <param name="contextProvider">an optional context provider</param>
    public Agent(ILanguageModel model, IContextProvider? contextProvider = null)
        : this(new JsonTranslator<T>(model), contextProvider)
    {
    }
    /// <summary>
    /// Create a new Agent that uses the given language model
    /// By default, the agent will use an in-memory transient message history.
    /// </summary>
    /// <param name="translator">Translator to use</param>
    /// <param name="contextProvider">Customize an object to use to capture this agent's interaction history</param>
    /// <exception cref="ArgumentNullException">If translator is null</exception>
    public Agent(JsonTranslator<T> translator, IContextProvider? contextProvider = null)
    {
        ArgumentVerify.ThrowIfNull(translator, nameof(translator));
        _translator = translator;
        _instructions = new Prompt();
        _builder = new PromptBuilder(translator.Model.ModelInfo.MaxCharCount / 2);
        _maxPromptLength = _builder.MaxLength;
        _contextProvider = contextProvider;
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
    /// <param name="requestMessage">request message</param>
    /// <param name="cancelToken">optional cancellation token</param>
    /// <returns>response message</returns>
    public async Task<Message> GetResponseMessageAsync(Message requestMessage, CancellationToken cancelToken = default)
    {
        ArgumentVerify.ThrowIfNull(requestMessage, nameof(requestMessage));

        string requestText = requestMessage.GetText();
        Prompt context = await BuildContextAsync(requestText, cancelToken);

        T response = await _translator.TranslateAsync(requestText, context, null, cancelToken).ConfigureAwait(false);
        Message responseMessage = Message.FromAssistant(response);

        OnReceivedResponse(requestMessage, responseMessage);

        return responseMessage;
    }

    /// <summary>
    /// Get a response to the given message
    /// </summary>
    /// <param name="request">request</param>
    /// <param name="cancelToken">optional cancel token</param>
    /// <returns>Response object</returns>
    public async Task<T> GetResponseAsync(Message request, CancellationToken cancelToken = default)
    {
        Message response = await GetResponseMessageAsync(request, cancelToken).ConfigureAwait(false);
        return response.GetBody<T>();
    }

    async Task<Prompt> BuildContextAsync(string requestText, CancellationToken cancelToken)
    {
        int requestLength = requestText.Length;
        //
        // Since are single threaded, we can keep reusing the same builder
        //
        _builder.Clear();
        _builder.MaxLength = (_maxPromptLength - requestLength);
        // Add any preamble       
        _builder.AddRange(_instructions);
        //
        // If a context provider is available, inject additional context
        //
        if (_contextProvider != null)
        {
            var context = _contextProvider.GetContextAsync(requestText, cancelToken);
            _builder.Add(PromptSection.Instruction("IMPORTANT CONTEXT for the user request:"));
            await AppendContextAsync(_builder, context);
        }
        return _builder.Prompt;
    }

    internal virtual Task<bool> AppendContextAsync(PromptBuilder builder, IAsyncEnumerable<IPromptSection> context)
    {
        return builder.AddRangeAsync(context);
    }

    protected virtual void OnReceivedResponse(Message request, Message response) { }
}
