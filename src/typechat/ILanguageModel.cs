// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public interface ILanguageModel
{
    Task<string> CompleteAsync(Prompt prompt, RequestSettings? settings, CancellationToken cancelToken);
}
