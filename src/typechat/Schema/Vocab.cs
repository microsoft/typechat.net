// Copyright (c) Microsoft. All rights reserved.

using System.Text;

namespace Microsoft.TypeChat.Schema;

/// <summary>
/// A Vocabulary is a string table whose values need not be hardcoded in code
/// Example: lists of product names
/// This means that each time a schema is generated, it can be different.
///
/// These string tables can be emitted in Typescript schema, e.g. like this:
/// type FourSeasons: 'Winter' | 'Spring' | 'Summer' | 'Fall'
///
/// The string tables can also be automatically validated to ensure the model sends back know strings.
/// Any error can be sent to the model for correction
/// 
/// </summary>
public interface IVocab : IEnumerable<VocabEntry>
{
    bool Contains(VocabEntry entry);
    bool Contains(VocabEntry entry, StringComparison comparison);
}

/// <summary>
/// An in-memory vocabulary.
/// </summary>
public class Vocab : List<VocabEntry>, IVocab
{
    const char DefaultEntrySeparator = '|';

    public Vocab() { }

    /// <summary>
    /// Make a copy of the source vocab
    /// </summary>
    /// <param name="src"></param>
    public Vocab(IVocab src)
    {
        ArgumentNullException.ThrowIfNull(src, nameof(src));
        foreach (var entry in src)
        {
            Add(entry);
        }
    }
    /// <summary>
    /// Initialize a vocabulary
    /// </summary>
    /// <param name="entries"></param>
    public Vocab(params string[] entries)
    {
        Add(entries);
    }

    /// <summary>
    /// Add entries to a vocabulary
    /// </summary>
    /// <param name="entries"></param>
    public void Add(params string[] entries)
    {
        if (!entries.IsNullOrEmpty())
        {

#if NET7_0_OR_GREATER
            EnsureCapacity(entries.Length);
#endif
            for (int i = 0; i < entries.Length; ++i)
            {
                base.Add(entries[i]);
            }
        }
    }

    public bool Contains(VocabEntry entry, StringComparison comparison)
    {
        if (string.IsNullOrEmpty(entry.Text))
        {
            return false;
        }

        for (int i = 0; i < Count; ++i)
        {
            if (this[i].CompareTo(entry, comparison) == 0)
            {
                return true;
            }
        }
        return false;
    }

    public static implicit operator Vocab(string[] values)
    {
        return new Vocab(values);
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        AppendTo(sb);
        return sb.ToString();
    }
    /// <summary>
    /// Append this vocabulary to the given StringBuilder. 
    /// </summary>
    /// <param name="sb"></param>
    /// <param name="quoteChar">How to quote individual entries</param>
    /// <param name="separator">Separator between strings</param>
    /// <returns></returns>
    public StringBuilder AppendTo(
        StringBuilder sb,
        char quoteChar = '\'',
        char separator = DefaultEntrySeparator)
    {
        sb ??= new StringBuilder();
        foreach (var entry in this)
        {
            if (sb.Length > 0)
            {
                sb.Append(' ').Append(separator).Append(' ');
            }
            if (quoteChar != char.MinValue)
            {
                sb.Append(quoteChar);
            }
            sb.Append(entry.Text);
            if (quoteChar != char.MinValue)
            {
                sb.Append(quoteChar);
            }
        }
        return sb;
    }

    /// <summary>
    /// Create a new vocabulary by parsing an initialization string like this:
    /// "Jane Austen | Charles Dickens | William Shakespeare | Toni Morisson"
    /// </summary>
    /// <param name="text"></param>
    /// <param name="separator"></param>
    /// <returns></returns>
    public static Vocab? Parse(string text, char separator = DefaultEntrySeparator)
    {
#if NET7_0_OR_GREATER
        string[] entries = text.Split(separator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
#else
        string[] entries = text.Split(separator).Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToArray();
#endif
        if (entries == null || entries.Length == 0)
        {
            return null;
        }
        Vocab vocab = new Vocab(entries);
        vocab.TrimExcess();
        return vocab;
    }
}

public static class VocabEx
{
    /// <summary>
    /// Return all strings in a vocab
    /// </summary>
    /// <param name="vocab"></param>
    /// <returns></returns>
    public static IEnumerable<string> Strings(this IVocab vocab)
    {
        return from entry in vocab
               select entry.Text;
    }
}
