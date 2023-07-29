// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

public struct DynamicVocabValue : IStringType
{
    public DynamicVocabValue()
    {
        _value = null;
        _vocabs = null;
    }

    internal DynamicVocabValue(IVocabCollection vocabs, string? value)
    {
        _vocabs = vocabs;
        _value = value;
    }

    string? _value;
    IVocabCollection? _vocabs;

    [JsonPropertyName("value")]
    public string? Value
    {
        get => _value;
        set
        {
            _value = value;
        }
    }

    internal IVocabCollection? Vocabs
    {
        get => _vocabs;
        set => _vocabs = value;
    }

    internal void ValidateConstraints(string vocabName, string? propertyName)
    {
        if (_vocabs != null)
        {
            _vocabs.ThrowIfNotInVocab(vocabName, propertyName, _value);
        }
    }

    public static implicit operator string(DynamicVocabValue value)
    {
        return value._value;
    }

}
public class DynamicVocabField
{
    string _vocabName;
    string? _propertyName;
    DynamicVocabValue _value;

    public DynamicVocabField(string vocabName, string? propertyName = null)
    {
        _vocabName = vocabName;
        _propertyName = propertyName;
    }

    void Set(DynamicVocabValue newValue)
    {
        newValue.ValidateConstraints(_vocabName, _propertyName);
        _value = newValue;
    }

    public DynamicVocabValue Value
    {
        get => _value;
        set
        {
            Set(value);
        }
    }

    public static implicit operator DynamicVocabValue(DynamicVocabField field)
    {
        return field._value;
    }

    public static implicit operator string(DynamicVocabField field)
    {
        return field._value;
    }
}

public class DynamicVocabValueConvertor : JsonConverter<DynamicVocabValue>
{
    IVocabCollection _vocabs;

    public DynamicVocabValueConvertor(IVocabCollection vocabs)
    {
        ArgumentNullException.ThrowIfNull(vocabs, nameof(vocabs));
        _vocabs = vocabs;
    }

    public override DynamicVocabValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? value = reader.GetString();
        return new DynamicVocabValue(_vocabs, value);
    }

    public override void Write(Utf8JsonWriter writer, DynamicVocabValue value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
