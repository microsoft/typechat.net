// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Tests;

public class MockLanguageModel : ILanguageModel
{
    ModelInfo _model;

    public MockLanguageModel()
    {
        _model = "Mock";
    }

    public ModelInfo ModelInfo => _model;

    public Task<string> CompleteAsync(Prompt prompt, TranslationSettings? settings, CancellationToken cancelToken = default)
    {
        return Task.FromResult("No comment");
    }
}
