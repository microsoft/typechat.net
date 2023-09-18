// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Dialog;

/// <summary>
/// A Message List.
/// Is also a message stream, and can be used to maintain interaction history for an agent
/// </summary>
public class MessageList : List<Message>, IMessageStream, IContextProvider
{
    /// <summary>
    /// Create a new message list
    /// </summary>
    public MessageList(int capacity = 4)
        : base(capacity)
    {
    }

    /// <summary>
    /// Number of messages in this message stream
    /// </summary>
    /// <returns>count</returns>
    public int GetCount() => Count;
    /// <summary>
    /// All messsages in this message stream
    /// </summary>
    /// <returns>An enumeration of messages</returns>
    public IEnumerable<Message> All() => this;

    /// <summary>
    /// Add a new message to this list
    /// </summary>
    /// <param name="message">message to add</param>
    /// <exception cref="ArgumentNullException">if message is null</exception>
    public new void Add(Message message)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }
        base.Add(message);
    }

    /// <summary>
    /// Append a message to the message stream
    /// </summary>
    /// <param name="message">message to append</param>
    public void Append(Message message) => Add(message);

    /// <summary>
    /// Return an enumeration of messages, most recent first
    /// </summary>
    /// <returns>enumeration of messages</returns>
    public IEnumerable<Message> Newest()
    {
        for (int i = Count - 1; i >= 0; --i)
        {
            yield return this[i];
        }
    }

    /// <summary>
    /// Just returns messages in order of newest first. You can build other message lists that support semantic and
    /// other forms of similarity
    /// supply
    /// </summary>
    /// <param name="request">find messages nearest to the given text</param>
    /// <returns>an enumeration of prompt sections</returns>
    public IEnumerable<IPromptSection> GetContext(string request)
    {
        return Newest();
    }

#pragma warning disable 1998
    /// <summary>
    /// Just returns messages in order of newest first. You can build other message lists that support semantic and
    /// </summary>
    /// <param name="request">find messages nearest to this</param>
    /// <param name="cancelToken">optional cancel token</param>
    /// <returns></returns>
    public async IAsyncEnumerable<IPromptSection>? GetContextAsync(string request, [EnumeratorCancellation] CancellationToken cancelToken)
    {
        for (int i = Count - 1; i >= 0; --i)
        {
            yield return this[i];
        }
    }
#pragma warning restore 1998

    /// <summary>
    /// Close the message stream. MessageList does nothing here
    /// </summary>
    public void Close() { }
}
