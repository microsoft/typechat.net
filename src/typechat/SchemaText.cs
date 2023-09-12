// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public struct SchemaText
{
    public static class Languages
    {
        public const string Typescript = "TypeScript";
    }

    string _lang;
    string _text;

    public SchemaText(string text)
        : this(text, Languages.Typescript)
    {
    }

    [JsonConstructor]
    public SchemaText(string text, string lang)
    {
        if (string.IsNullOrEmpty(text))
        {
            throw new ArgumentException("Text cannot be null or empty.", nameof(text));
        }
        if (string.IsNullOrEmpty(lang))
        {
            throw new ArgumentException("Lanauge cannot be null or empty.", nameof(lang));
        }

        _lang = lang;
        _text = text;
    }

    [JsonPropertyName("lang")]
    public string Lang => _lang;
    [JsonPropertyName("text")]
    public string Text => _text;

    public static implicit operator string(SchemaText schema)
    {
        return schema._text;
    }

    public static SchemaText Load(string filePath)
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
        return new SchemaText(schemaText, lang);
    }
}
