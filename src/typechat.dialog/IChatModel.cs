// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public interface IChatMessage
{
    string From { get; }
    string GetText();
}

public interface IChatModel
{
    Task<string> GetResponseAsync(IChatMessage message, IEnumerable<IChatMessage>? context, RequestSettings? settings, CancellationToken cancelToken);
}
