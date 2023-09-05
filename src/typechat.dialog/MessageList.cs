// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Dialog;

public class MessageList : List<Message>, IMessageStream
{
    public MessageList() { }

    public int GetCount() => Count;
    public IEnumerable<Message> All() => this;

    public new void Add(Message message)
    {
        ArgumentNullException.ThrowIfNull(message, nameof(message));
        base.Add(message);
    }

    public void Append(Message message) => Add(message);

    public IEnumerable<Message> Newest()
    {
        for (int i = Count - 1; i >= 0; --i)
        {
            yield return this[i];
        }
    }

    /// <summary>
    /// By default, just returns the newest messages
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public virtual IEnumerable<Message> Nearest(string request)
    {
        return Newest();
    }

    public void Close() { }
}
