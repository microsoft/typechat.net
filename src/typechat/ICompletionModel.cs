// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public interface ICompletionModel
{
    Task<string> CompleteAsync(string prompt, RequestSettings? settings, CancellationToken cancelToken);
}
