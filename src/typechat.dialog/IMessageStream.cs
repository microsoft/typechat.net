// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public interface IMessageStream
{
    void Append(Message message);
    IEnumerable<Message> All();

    IEnumerable<Message> Newest();

    void Clear();
    void Close();
}

public interface IMessageStreamAsync
{
    Task AppendAsync(Message message, CancellationToken cancelToken = default);
    IAsyncEnumerable<Message> AllAsync();
    IAsyncEnumerable<Message> NewestAsync();
    Task ClearAsync();
    Task CloseAsync();
}

