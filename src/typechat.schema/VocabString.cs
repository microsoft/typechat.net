// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

public struct VocabString : IStringType
{
    public VocabString()
    {
        Value = null;
        VocabName = null;
    }

    public VocabString(string vocabName, string value)
    {
        VocabName = vocabName;
        Value = value;
    }

    [JsonPropertyName("value")]
    public string? Value { get; set; }

    [JsonPropertyName("vocab")]
    public string VocabName { get; set; }

    public void ValidateConstraints(IVocab vocab, string? propertyName = null)
    {
        vocab.ThrowIfNotInVocab(propertyName, Value);
    }

    public void ValidateConstraints(IVocabCollection vocabs, string? propertyName = null)
    {
        vocabs.ThrowIfNotInVocab(VocabName, propertyName, Value);
    }

    public static implicit operator string(VocabString value)
    {
        return value.Value;
    }
}

public class VocabStringJsonConvertor : JsonConverter<VocabString>
{
    IVocabCollection _vocabs;

    public VocabStringJsonConvertor(IVocabCollection vocabs)
    {
        ArgumentNullException.ThrowIfNull(vocabs, nameof(vocabs));
        _vocabs = vocabs;
    }

    public IVocabCollection Vocabs => _vocabs;

    public override VocabString Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var convertor = (JsonConverter<VocabString>)options.GetConverter(typeof(VocabString));
        var vocabString = convertor.Read(ref reader, typeToConvert, options);
        vocabString.ValidateConstraints(_vocabs);
        return vocabString;
    }

    public override void Write(Utf8JsonWriter writer, VocabString value, JsonSerializerOptions options)
    {
        var convertor = (JsonConverter<VocabString>)options.GetConverter(typeof(VocabString));
        convertor.Write(writer, value, options);
    }
}

