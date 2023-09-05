// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Dialog;

public interface IMessageStream
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

