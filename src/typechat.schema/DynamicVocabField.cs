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

public class DynamicVocabField
{
    string _vocabName;
    string _propertyName;
    VocabString _value;

    public DynamicVocabField(string vocabName, string propertyName)
    {
        _vocabName = vocabName;
        _propertyName = propertyName;
    }

    void Set(VocabString newValue)
    {
        newValue.ValidateConstraints(_vocabName, _propertyName);
        _value = newValue;
    }

    public VocabString Value
    {
        get => _value;
        set
        {
            Set(value);
        }
    }

    public static implicit operator VocabString(DynamicVocabField field)
    {
        return field._value;
    }

    public static implicit operator string(DynamicVocabField field)
    {
        return field._value;
    }
}

public class DynamicVocabValueConvertor : JsonConverter<VocabString>
{
    IVocabCollection _vocabs;

    public DynamicVocabValueConvertor(IVocabCollection vocabs)
    {
        ArgumentNullException.ThrowIfNull(vocabs, nameof(vocabs));
        _vocabs = vocabs;
    }

    public override VocabString Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? value = reader.GetString();
        return new VocabString(_vocabs, value);
    }

    public override void Write(Utf8JsonWriter writer, VocabString value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
