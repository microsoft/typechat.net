// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

/// <summary>
/// A backing field for strings that must be from a particular vocabulary
/// </summary>
public struct VocabField
{
    IVocab _vocab;
    string? _propertyName;
    string? _value;

    public VocabField(string vocab, string? propertyName = null)
        : this(Vocab.Parse(vocab), propertyName)
    {
    }

    public VocabField(IVocab vocab, string? propertyName = null)
    {
        ArgumentNullException.ThrowIfNull(vocab, nameof(vocab));
        _vocab = vocab;
        _propertyName = propertyName;
        _value = null;
    }

    [JsonPropertyName("value")]
    public string Value
    {
        get => _value;
        set
        {
            ArgumentNullException.ThrowIfNull(value, nameof(Value));
            _vocab.ThrowIfNotInVocab(_propertyName, value);
            _value = value;
        }
    }

    public static implicit operator string(VocabField field)
    {
        return field._value;
    }
}
