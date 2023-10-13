// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

internal class VocabStringJsonConvertor : JsonConverter<VocabString>
{
    public VocabStringJsonConvertor(IVocabCollection vocabs)
    {
        ArgumentVerify.ThrowIfNull(vocabs, nameof(vocabs));
        Vocabs = vocabs;
    }

    public IVocabCollection Vocabs { get; }

    public override VocabString Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var convertor = (JsonConverter<VocabString>)options.GetConverter(typeof(VocabString));
        var vocabString = convertor.Read(ref reader, typeToConvert, options);
        vocabString.ValidateConstraints(Vocabs);
        return vocabString;
    }

    public override void Write(Utf8JsonWriter writer, VocabString value, JsonSerializerOptions options)
    {
        var convertor = (JsonConverter<VocabString>)options.GetConverter(typeof(VocabString));
        convertor.Write(writer, value, options);
    }
}

