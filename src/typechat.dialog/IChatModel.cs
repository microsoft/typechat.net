// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public interface IChatModel
{
    Task<string> GetResponseAsync(IMessage message, IEnumerable<IMessage>? context, RequestSettings? settings, CancellationToken cancelToken);
}
