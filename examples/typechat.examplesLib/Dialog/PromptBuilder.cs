// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// Prompts have a maximum length. Prompt lengths are be limited by model capacity or policy
/// PromptBuilder builds prompts consisting of multiple prompt sections in a way that the prompt
/// length does not exceed a given maximum
/// </summary>
public class PromptBuilder
{
    private int _maxLength;
    private Func<string, int, string>? _substring;

    /// <summary>
    /// Create a builder to create prompts whose length does not exceed maxLength characters
    /// </summary>
    /// <param name="maxLength">maximum length</param>
    public PromptBuilder(int maxLength)
        : this(maxLength, Substring)
    {
    }

    /// <summary>
    /// Create a builder to create prompts whose length does not exceed maxLength characters
    /// If a full prompt section is too long, inovokes a substringExtractor callback to extract a
    /// suitable substring, if any. Substring extractors could do so at sentence boundaries, paragraph
    /// boundaries and so on.
    /// </summary>
    /// <param name="maxLength">Prompt will not exceed this maxLengthin characters</param>
    /// <param name="substringExtractor">optional extractor</param>
    public PromptBuilder(int maxLength, Func<string, int, string>? substringExtractor = null)
    {
        _maxLength = maxLength;
        _substring = substringExtractor;
    }

    /// <summary>
    /// The prompt being built
    /// </summary>
    public Prompt Prompt { get; } = new();

    /// <summary>
    /// Current length of the prompt in characters
    /// </summary>
    public int Length { get; private set; } = 0;

    /// <summary>
    /// Maximum allowed prompt length
    /// </summary>
    public int MaxLength
    {
        get => _maxLength;
        set
        {
            if (value < Length)
            {
                throw new ArgumentException($"Current Length: {Length} exceeds {value}");
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
        if (section is null)
        {
            throw new ArgumentNullException(nameof(section));
        }

        string text = section.GetText();
        if (string.IsNullOrEmpty(text))
        {
            return true;
        }

        int lengthAvailable = _maxLength - Length;
        if (text.Length <= lengthAvailable)
        {
            Prompt.Append(section);
            Length += text.Length;
            return true;
        }

        if (_substring is not null)
        {
            text = _substring(text, lengthAvailable);
            Prompt.Append(section.Source, text);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Add a collection of prompt sections - until the MaxLength - is hit
    /// </summary>
    /// <param name="sections">sections to add</param>
    /// <returns>true if all sections were added</returns>
    public bool AddRange(IEnumerable<IPromptSection> sections)
    {
        ArgumentVerify.ThrowIfNull(sections, nameof(sections));

        foreach (var section in sections)
        {
            if (!Add(section))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Add collection of prompt sections - until the MaxLength - is hit
    /// </summary>
    /// <param name="sections"></param>
    /// <returns></returns>
    public async Task<bool> AddRangeAsync(IAsyncEnumerable<IPromptSection> sections, CancellationToken cancelToken = default)
    {
        ArgumentVerify.ThrowIfNull(sections, nameof(sections));

        await foreach (var section in sections.WithCancellation(cancelToken).ConfigureAwait(false))
        {
            if (!Add(section))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Clear the builder
    /// </summary>
    public void Clear()
    {
        Prompt.Clear();
        Length = 0;
    }

    /// <summary>
    /// Reverse the order of sections in the builder, in the given range
    /// </summary>
    /// <param name="startAt"></param>
    /// <param name="count"></param>
    public void Reverse(int startAt, int count)
    {
        Reverse(startAt, count);
    }

    static string Substring(string text, int length)
    {
        return text.Substring(0, length);
    }
}
