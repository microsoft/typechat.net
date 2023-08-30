// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public interface IChatModel
{
    Task<Message> GetResponseAsync(Message message, IEnumerable<Message>? context, RequestSettings? settings, CancellationToken cancelToken);
}
