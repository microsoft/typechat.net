// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public class PromptSection : IPromptSection
{
    public static class Sources
    {
        public const string System = "system";
        public const string User = "user";
        public const string Assistant = "assistant";
    }

    public static readonly PromptSection Empty = FromUser(string.Empty);

    string _source;
    string _text;

    public PromptSection(string text)
        : this(Sources.User, text)
    {
    }

    public PromptSection(string source, string text)
    {
        ArgumentException.ThrowIfNullOrEmpty(source, nameof(source));
        ArgumentNullException.ThrowIfNull(text, nameof(text));
        _source = source;
        SetText(text);
    }

    public string? Source => _source;
    public string GetText() => _text;

    public void SetText(string text)
    {
        ArgumentNullException.ThrowIfNull(text, nameof(text));
        _text = text;
    }

    public override string ToString()
    {
        return $"{_source}: {_text}";
    }

    public static implicit operator PromptSection(string text) => FromUser(text);

    public static PromptSection FromSystem(string text) => new PromptSection(Sources.System, text);
    public static PromptSection FromUser(string text) => new PromptSection(Sources.User, text);
    public static PromptSection FromAssistant(string text) => new PromptSection(Sources.Assistant, text);
}
