// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Tests;

public class MockLanguageModel : ILanguageModel
{
    private readonly ModelInfo _model;

    public MockLanguageModel()
    {
        _model = "Mock";
    }

    public ModelInfo ModelInfo => _model;

    public Task<LanguageModelResponse> CompleteAsync(Prompt prompt, TranslationSettings? settings, CancellationToken cancelToken)
    {
        return Task.FromResult<LanguageModelResponse>("No comment");
    }
}
