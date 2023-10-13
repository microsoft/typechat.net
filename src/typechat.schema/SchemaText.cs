// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

/// <summary>
/// Schema is defined by:
/// - The schema specification, in text
/// - The language the schema is written in, such as Typescript
/// </summary>
public struct SchemaText
{
    /// <summary>
    /// Standard schema languages
    /// </summary>
    public static class Languages
    {
        /// <summary>
        /// The schema is written in Typescript
        /// </summary>
        public const string Typescript = "TypeScript";
    }

    /// <summary>
    /// Create a SchemaText object to hold schema specified in the given language
    /// </summary>
    /// <param name="text">schema text</param>
    /// <param name="lang">schema language</param>
    /// <exception cref="ArgumentException"></exception>
    [JsonConstructor]
    public SchemaText(string text, string lang)
    {
        ArgumentVerify.ThrowIfNullOrEmpty(text, nameof(text));
        ArgumentVerify.ThrowIfNullOrEmpty(lang, nameof(lang));

        Lang = lang;
        Text = text;
    }

    /// <summary>
    /// The schema language
    /// </summary>
    [JsonPropertyName("lang")]
    public string Lang { get; }

    /// <summary>
    /// The schema text
    /// </summary>
    [JsonPropertyName("text")]
    public string Text { get; }

    public override string ToString()
    {
        return Text;
    }

    public static implicit operator string(SchemaText schema)
    {
        return schema.Text;
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
