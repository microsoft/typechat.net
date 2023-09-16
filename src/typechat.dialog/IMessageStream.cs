// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Dialog;

/// <summary>
/// A stream of messages between Agents
/// </summary>
public interface IMessageStream : IContextProvider
{
    int GetCount();

    void Append(Message message);
    IEnumerable<Message> All();
    IEnumerable<Message> Newest();
    /// <summary>
    /// Return messages that are nearest to the given message
    /// </summary>
    /// <param name="message"></param>
    /// <returns>Ranked messages, in order</returns>
    IEnumerable<Message> Nearest(string request);

    void Clear();
    void Close();
}

