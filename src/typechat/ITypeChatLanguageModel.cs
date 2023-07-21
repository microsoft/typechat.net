// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public interface ITypeChatLanguageModel
{
    Task<string> CompleteAsync(string prompt, RequestSettings? settings, CancellationToken cancelToken);
}
