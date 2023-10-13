// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

internal struct VocabString : IStringType
{
    public VocabString()
        : this(null, null)
    {
    }

    public VocabString(string? vocabName, string? value)
    {
        VocabName = vocabName;
        Value = value;
    }

    [JsonPropertyName("value")]
    public string? Value { get; set; }

    [JsonPropertyName("vocab")]
    public string? VocabName { get; set; }

    public void ValidateConstraints(IVocab vocab, string? propertyName = null)
        => vocab.ThrowIfNotInVocab(propertyName, Value);

    public void ValidateConstraints(IVocabCollection vocabs, string? propertyName = null)
        => vocabs.ThrowIfNotInVocab(VocabName, propertyName, Value);

    public static implicit operator string(VocabString value)
        => value.Value;
}

