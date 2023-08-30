// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public interface IChatModel
{
    Task<ChatMessage> GetResponseAsync(ChatMessage message, IEnumerable<ChatMessage>? context, RequestSettings? settings, CancellationToken cancelToken);
}
