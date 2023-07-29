// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

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
