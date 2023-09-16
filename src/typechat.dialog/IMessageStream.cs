// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Dialog;

/// <summary>
/// An append stream of messages between Agents
/// </summary>
public interface IMessageStream : IContextProvider
{
    /// <summary>
    /// Number of messages in the stream
    /// </summary>
    /// <returns>count</returns>
    int GetCount();

    /// <summary>
    /// Append a message to the stream
    /// </summary>
    /// <param name="message">message to append</param>
    void Append(Message message);
    /// <summary>
    /// Return all messages in the stream
    /// </summary>
    /// <returns></returns>
    IEnumerable<Message> All();
    /// <summary>
    /// Return the newest messages in the stream in order - most recent messages first
    /// </summary>
    /// <returns></returns>
    IEnumerable<Message> Newest();
    /// <summary>
    /// Return messages that are semantically nearest to the given message
    /// </summary>
    /// <param name="message"></param>
    /// <returns>Ranked messages, in order</returns>
    IEnumerable<Message> Nearest(string request);
    /// <summary>
    /// Clear the stream
    /// </summary>
    void Clear();
    /// <summary>
    /// Close the stream
    /// </summary>
    void Close();
}

