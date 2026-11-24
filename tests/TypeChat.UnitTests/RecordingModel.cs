// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.UnitTests;

internal sealed record RecordedSection(string? Role, string Text, IPromptSection Original);

internal sealed class RecordingModel(params string[] replies) : ILanguageModel
{
    private readonly Queue<string> _replies = new(replies);

    public ModelInfo ModelInfo => "recorder";
    public List<RecordedSection[]> Calls { get; } = [];

    public Task<string> CompleteAsync(Prompt prompt, TranslationSettings? settings, CancellationToken cancelToken)
    {
        // Translation appends/trims repair turns in place. Snapshot the actual model input at each call.
        Calls.Add(prompt.Select(s => new RecordedSection(s.Source, s.GetText(), s)).ToArray());
        return Task.FromResult(_replies.Dequeue());
    }
}
