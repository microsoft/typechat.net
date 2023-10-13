// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Dialog;

/// <summary>
/// An Agent that uses a JsonTranslator to create strongly typed responses to user requests. 
/// 
/// The Agent automatically maintains interaction history. For every request, the agent
/// retrieves pertinent context from this history and includes it in the request. You can control
/// the size of each request by setting the MaxRequestPromptLength property.
/// 
/// </summary>
public class AgentWithHistory<T> : Agent<T>
{
    IMessageStream _history;

    /// <summary>
    /// Create a new Agent that uses the given language model
    /// By default, the agent will use an in-memory transient message history.
    /// </summary>
    /// <param name="model">language model the agent uses</param>
    public AgentWithHistory(ILanguageModel model)
        : this(new JsonTranslator<T>(model))
    {
    }

    /// <summary>
    /// Create a new Agent that uses the given language model
    /// By default, the agent will use an in-memory transient message history.
    /// </summary>
    /// <param name="translator">Translator to use</param>
    public AgentWithHistory(JsonTranslator<T> translator)
        : this(translator, new MessageList())
    {

    }

    /// <summary>
    /// Create a new Agent that uses the given language model
    /// This agent uses a message history you provide. This history can be persistent
    /// </summary>
    /// <param name="translator">Translator to use</param>
    /// <param name="history">object to store interaction history</param>
    /// <exception cref="ArgumentNullException">If translator is null</exception>
    public AgentWithHistory(JsonTranslator<T> translator, IMessageStream history)
        : base(translator, history)
    {
        _history = history;
    }

    /// <summary>
    /// This Agent's interaction history. The Agent automatically uses this history to inject
    /// context with every request.
    /// </summary>
    public IMessageStream History => _history;

    /// <summary>
    /// Customize how a response is transformed into a message written into history
    /// If you return null, the response will not be added to the history
    /// </summary>
    public Func<T, Message?> CreateMessageForHistory { get; set; }

    protected override Task<bool> AppendContextAsync(PromptBuilder builder, IAsyncEnumerable<IPromptSection> context)
    {
        return builder.AddHistoryAsync(context);
    }

    /// <summary>
    /// This is where we append messages to history
    /// - User message is saved as is
    /// - The response is further (optionally) transformed and then saved
    /// </summary>
    /// <param name="request">user request</param>
    /// <param name="preparedRequest"></param>
    /// <param name="response"></param>
    /// <returns></returns>
    protected async override Task ReceivedResponseAsync(Message request, Message preparedRequest, Message response)
    {
        await _history.AppendAsync(request).ConfigureAwait(false);
        Message? historyMessage = (CreateMessageForHistory != null) ?
                                  CreateMessageForHistory(response.GetBody<T>()) :
                                  response;
        if (historyMessage != null)
        {
            await _history.AppendAsync(historyMessage).ConfigureAwait(false);
        }
    }
}
