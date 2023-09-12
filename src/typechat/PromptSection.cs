// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public class PromptSection : IPromptSection
{
    public static class Sources
    {
        /// <summary>
        /// This section contains text supplied by the system
        /// </summary>
        public const string System = "system";
        /// <summary>
        /// This section contains text supplied by the user
        /// </summary>
        public const string User = "user";
        /// <summary>
        /// This section contains text produced by an AI assistant or model
        /// </summary>
        public const string Assistant = "assistant";
    }

    public static readonly PromptSection Empty = FromUser(string.Empty);

    string _source;
    string _text;

    public PromptSection()
        : this(string.Empty)
    {
    }

    public PromptSection(string text)
        : this(Sources.User, text)
    {
    }

    public PromptSection(string source, string text)
    {
        if (string.IsNullOrEmpty(source))
        {
            throw new ArgumentException("Source cannot be null or empty.", nameof(source));
        }

        if (text == null)
        {
            throw new ArgumentNullException(nameof(text));
        }

        _source = source;
        SetText(text);
    }

    public string? Source => _source;
    public string GetText() => _text;
    public bool IsEmpty => string.IsNullOrEmpty(_text);

    public void SetText(string text)
    {
        if (text == null)
        {
            throw new ArgumentNullException(nameof(text));
        }
        _text = text;
    }

    public void AppendText(string text)
    {
        if (IsEmpty)
        {
            _text = text;
        }
        else
        {
            _text += text;
        }
    }

    public override string ToString()
    {
        return $"{_source}: {_text}";
    }

    public static implicit operator PromptSection(string text) => FromUser(text);

    public static PromptSection FromSystem(string text) => new PromptSection(Sources.System, text);
    public static PromptSection FromUser(string text) => new PromptSection(Sources.User, text);
    public static PromptSection FromAssistant(string text) => new PromptSection(Sources.Assistant, text);

    public static PromptSection operator +(PromptSection section, string text)
    {
        section.AppendText(text);
        return section;
    }
}
