// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

/// <summary>
/// A custom convertor used with the Json Serializer to implement vocabuaries
/// </summary>
internal class JsonVocabConvertor : JsonConverter<string?>
{
    string? _propertyName;
    string? _vocabName;
    IVocab _vocab;

    public JsonVocabConvertor() { }

    internal JsonVocabConvertor(string vocabName, string? propertyName)
    {
        _propertyName = propertyName;
        _vocabName = vocabName;
    }

    public JsonVocabConvertor(IVocab vocab, string? propertyName)
    {
        ArgumentVerify.ThrowIfNull(vocab, nameof(vocab));
        _propertyName = propertyName;
        _vocab = vocab;
    }

    internal bool HasVocabName => !string.IsNullOrEmpty(_vocabName);

    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? value = reader.GetString();
        if (_vocab != null)
        {
            _vocab.ThrowIfNotInVocab(_propertyName, value);
            return value;
        }
        else if (HasVocabName)
        {
            // Dynamicaly bind to a vocabulary if possible
            var converter = options.GetConverter(typeof(VocabString)) as VocabStringJsonConvertor;
            if (converter == null)
            {
                throw new SchemaException(SchemaException.ErrorCode.VocabNotFound);
            }
            converter.Vocabs.ThrowIfNotInVocab(_vocabName, _propertyName, value);
        }
        return value;
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}

