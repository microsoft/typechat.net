// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

/// <summary>
/// A backing field for strings that must be from a particular vocabulary
/// </summary>
public struct VocabField
{
    VocabString _value;
    string? _propertyName;

    public VocabField(string vocab, string? propertyName = null)
        : this(Vocab.Parse(vocab), propertyName)
    {
    }

    public VocabField(IVocab vocab, string? propertyName = null)
    {
        ArgumentNullException.ThrowIfNull(vocab, nameof(vocab));
        _value = new VocabString(vocab, null);
        _propertyName = propertyName;
    }

    [JsonPropertyName("value")]
    public string Value
    {
        get => _value;
        set
        {
            ArgumentNullException.ThrowIfNull(value, nameof(Value));
            _value.Set(_propertyName, value);
        }
    }

    public static implicit operator string(VocabField field)
    {
        return field._value;
    }
}
