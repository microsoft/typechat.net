// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public interface ILanguageModel
{
    ModelInfo ModelInfo { get; }

    Task<string> CompleteAsync(Prompt prompt, TranslationSettings? settings, CancellationToken cancelToken);
}
