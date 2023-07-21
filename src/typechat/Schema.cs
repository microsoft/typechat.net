// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public struct Schema
{
    public static class Languages
    {
        public const string Typescript = "TypeScript";
    }

    string _lang;
    string _text;

    public Schema(string text)
        : this(text, Languages.Typescript)
    {
    }

    [JsonConstructor]
    public Schema(string text, string lang)
    {
        ArgumentException.ThrowIfNullOrEmpty(text, nameof(text));
        ArgumentException.ThrowIfNullOrEmpty(lang, nameof(lang));

        _lang = lang;
        _text = text;
    }

    [JsonPropertyName("lang")]
    public string Lang => _lang;
    [JsonPropertyName("text")]
    public string Text => _text;

    public static implicit operator string(Schema schema)
    {
        return schema._text;
    }

    public static Schema Load(string filePath)
    {
        string schemaText = File.ReadAllText(filePath);
        string ext = Path.GetExtension(filePath);
        string lang = null;
        if (ext == ".ts")
        {
            lang = Languages.Typescript;
        }
        else
        {
            throw new NotSupportedException($"{lang} is not supported");
        }
        return new Schema(schemaText, lang);
    }
}
