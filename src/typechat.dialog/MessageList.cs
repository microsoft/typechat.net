// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public class MessageList : List<Message>, IMessageStream
{
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

    public void Close() { }

    public void Trim(int trimCount)
    {
        if (trimCount > Count)
        {
            Clear();
        }
        else
        {
            RemoveRange(Count - trimCount, trimCount);
        }
    }
}
