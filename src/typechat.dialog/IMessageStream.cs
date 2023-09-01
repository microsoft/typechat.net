// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Dialog;

public interface IMessageStream
{
    void Append(Message message);
    IEnumerable<Message> All();
    IEnumerable<Message> Newest();
    void Clear();
    void Close();
}

