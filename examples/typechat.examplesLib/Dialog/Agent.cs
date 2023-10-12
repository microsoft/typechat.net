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
        // By default, only use 1/2 the estimated # of characters the model supports.. for prompts
        // the Agent sends
        _maxPromptLength = translator.Model.ModelInfo.MaxCharCount / 2;
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
        string preparedRequestText = requestText;
        //
        // Prepare the actual message to send to the model
        //
        Message preparedRequestMessage = await PrepareRequestAsync(requestMessage, cancelToken).ConfigureAwait(false);
        if (!object.ReferenceEquals(preparedRequestMessage, requestMessage))
        {
            preparedRequestText = preparedRequestMessage.GetText();
        }
        //
        // Prepare the context to send. For context building, use the original request text
        //
        Prompt context = await BuildContextAsync(requestText, preparedRequestText.Length, cancelToken).ConfigureAwait(false);
        //
        // Translate
        //
        T response = await _translator.TranslateAsync(preparedRequestText, context, null, cancelToken).ConfigureAwait(false);

        Message responseMessage = Message.FromAssistant(response);

        await ReceivedResponseAsync(requestMessage, preparedRequestMessage, responseMessage).ConfigureAwait(false);

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

    async Task<Prompt> BuildContextAsync(string requestText, int actualRequestLength, CancellationToken cancelToken)
    {
        PromptBuilder builder = CreateBuilder(_maxPromptLength - actualRequestLength);
        // Add any preamble       
        builder.AddRange(_instructions);
        //
        // If a context provider is available, inject additional context
        //
        if (_contextProvider != null)
        {
            var context = _contextProvider.GetContextAsync(requestText, cancelToken);
            builder.Add(PromptSection.Instruction("IMPORTANT CONTEXT for the user request:"));
            await AppendContextAsync(builder, context).ConfigureAwait(false);
        }
        return builder.Prompt;
    }

    /// <summary>
    /// Override to customize the actual message sent to the model. Several scenarios involve
    /// transforming the user's message in various ways first
    /// By default, the request message is sent to the model as is
    /// </summary>
    /// <param name="request">request message</param>
    /// <param name="cancelToken">cancel </param>
    /// <returns>Actual text to send to the AI</returns>
    protected virtual Task<Message> PrepareRequestAsync(Message request, CancellationToken cancelToken)
    {
        return Task.FromResult(request);
    }
    /// <summary>
    /// Override to customize how user context is added to the given prompt builder
    /// Since the builder will limit the # of characters, you may want to do some pre processing
    /// </summary>
    /// <param name="builder">builder to append to</param>
    /// <param name="context">context to append</param>
    /// <returns></returns>
    protected virtual Task<bool> AppendContextAsync(PromptBuilder builder, IAsyncEnumerable<IPromptSection> context)
    {
        return builder.AddRangeAsync(context);
    }
    /// <summary>
    /// Invoked when a valid response was received - the response is placed in the message body
    /// </summary>
    /// <param name="request">request message</param>
    /// <param name="preparedRequest">the prepared request that was actually used in translation</param>
    /// <param name="response">response message</param>
    /// <returns></returns>
    protected virtual Task ReceivedResponseAsync(Message request, Message preparedRequest, Message response)
    {
        return Task.CompletedTask;
    }

    PromptBuilder CreateBuilder(int maxLength)
    {
        // Future: Pool these
        return new PromptBuilder(maxLength);
    }
}
