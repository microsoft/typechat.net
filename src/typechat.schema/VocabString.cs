// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

public struct VocabString : IStringType
{
    public VocabString()
    {
        _value = null;
        _vocab = null;
        _vocabs = null;
    }

    public VocabString(IVocab vocab, string value)
    {
        _vocab = vocab;
        _value = value;
    }

    internal VocabString(IVocabCollection vocabs, string? value)
    {
        _vocabs = vocabs;
        _value = value;
    }

    string? _value;
    IVocabCollection? _vocabs;
    IVocab? _vocab;

    [JsonPropertyName("value")]
    public string? Value
    {
        get => _value;
        set
        {
            _value = value;
        }
    }

    [JsonIgnore]
    public IVocab? Vocab
    {
        get => _vocab;
    }

    internal IVocabCollection? Vocabs
    {
        get => _vocabs;
        set => _vocabs = value;
    }

    public void ValidateConstraints(string? propertyName = null)
    {
        _vocab.ThrowIfNotInVocab(propertyName, _value);
    }

    internal void Set(string? propertyName, string value)
    {
        _vocab.ThrowIfNotInVocab(propertyName, value);
        _value = value;
    }

    internal void BindVocab(string vocabName)
    {
        _vocab = _vocabs?.Get(vocabName)?.Vocab;
    }

    internal void ValidateConstraints(string vocabName, string? propertyName = null)
    {
        BindVocab(vocabName);
        _vocab.ThrowIfNotInVocab(propertyName, _value);
    }

    public static implicit operator string(VocabString value)
    {
        return value._value;
    }
}
