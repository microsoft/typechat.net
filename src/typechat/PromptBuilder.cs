// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// Prompts have a maximum length. Prompt lengths are be limited by model capacity or policy
/// PromptBuilder builds prompts consisting of multiple prompt sections in a way that the prompt
/// length does not exceeed a given maximum
/// </summary>
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

    /// <summary>
    /// Create a new prompt builder
    /// </summary>
    /// <param name="maxLength">Prompt will not exceed this maxLengthin characters</param>
    /// <param name="substringExtractor">If a full prompt section is too long, this callback can extract a suitable substring</param>
    public PromptBuilder(int maxLength, Func<string, int, string>? substringExtractor)
    {
        _prompt = new Prompt();
        _maxLength = maxLength;
        _substring = substringExtractor;
    }

    public Prompt Prompt => _prompt;
    public int Length => _currentLength;
    public int MaxLength
    {
        get => _maxLength;
        set
        {
            if (value < _currentLength)
            {
                throw new ArgumentException($"CurrentLength: {_currentLength} exceeds {value}");
            }
            _maxLength = value;
        }
    }

    /// <summary>
    /// Add a prompt section if the total length of the prompt will not exceed limits
    /// </summary>
    /// <param name="text">text to add</param>
    /// <returns>true if added, false if not</returns>
    public bool Add(string text)
    {
        return Add(new PromptSection(text));
    }

    /// <summary>
    /// Add a prompt section if the total length of the prompt will not exceed limits
    /// </summary>
    /// <param name="section">section to add</param>
    /// <returns>true if added, false if not</returns>
    public bool Add(IPromptSection section)
    {
        if (section == null)
        {
            throw new ArgumentNullException(nameof(section));
        }

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

    public bool AddRange(IEnumerable<IPromptSection> sections)
    {
        if (sections == null)
        {
            throw new ArgumentNullException(nameof(sections));
        }
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

    public void Reverse(int startAt, int count)
    {
        Reverse(startAt, count);
    }

    static string Substring(string text, int length)
    {
        return text.Substring(0, length);
    }
}
