// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public class PromptBuilder
{
    Prompt _prompt;
    int _currentLength;
    int _maxLength;
    Func<string, int, string>? _substring;

    public PromptBuilder(int maxLength)
        : this(maxLength, Substring)
    {
    }

    public PromptBuilder(int maxLength, Func<string, int, string>? substringExtractor)
    {
        _prompt = new Prompt();
        _maxLength = maxLength;
        _substring = substringExtractor;
    }

    public Prompt Prompt => _prompt;
    public int Length => _currentLength;
    public int MaxLength => _maxLength;

    public bool Add(string text)
    {
        return Add(new PromptSection(text));
    }

    public bool Add(IPromptSection section)
    {
        ArgumentNullException.ThrowIfNull(section, nameof(section));

        string text = section.GetText();
        if (string.IsNullOrEmpty(text))
        {
            return true;
        }

        int lengthAvailable = _maxLength - _currentLength;
        if (text.Length <= lengthAvailable)
        {
            _prompt.Push(section);
            _currentLength += text.Length;
            return true;
        }
        if (_substring != null)
        {
            text = _substring(text, lengthAvailable);
            _prompt.Push(section.Source, text);
            return true;
        }
        return false;
    }

    public bool Add(IEnumerable<IPromptSection> sections)
    {
        ArgumentNullException.ThrowIfNull(sections, nameof(sections));
        foreach (var section in sections)
        {
            if (!Add(section))
            {
                return false;
            }
        }
        return true;
    }

    public void Clear()
    {
        _prompt.Clear();
        _currentLength = 0;
    }

    static string Substring(string text, int length)
    {
        return text.Substring(0, length);
    }
}
