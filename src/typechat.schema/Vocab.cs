// Copyright (c) Microsoft. All rights reserved.

using System.Text;

namespace Microsoft.TypeChat.Schema;

/// <summary>
/// A Vocabulary is a string table whose values need not be hardcoded in code
/// Example: lists of product names
/// This means that each time a schema is generated, it can be different.
/// </summary>
public interface IVocab : IEnumerable<VocabEntry>
{
    bool Contains(VocabEntry entry);
}

/// <summary>
/// An in-memory vocabulary. 
/// </summary>
public class Vocab : List<VocabEntry>, IVocab
{
    public const char DefaultSeparator = '|';

    public Vocab() { }

    public Vocab(IVocab src)
    {
        ArgumentNullException.ThrowIfNull(src, nameof(src));
        foreach (var entry in src)
        {
            Add(entry);
        }
    }

    public Vocab(params string[] entries)
    {
        Add(entries);
    }

    public Vocab(IEnumerable<string> entries)
    {
        foreach (var entry in entries)
        {
            Add(entry);
        }
    }

    public void Add(params string[] entries)
    {
        if (!entries.IsNullOrEmpty())
        {
            base.EnsureCapacity(entries.Length);
            for (int i = 0; i < entries.Length; ++i)
            {
                base.Add(entries[i]);
            }
        }
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

    public StringBuilder AppendTo(
        StringBuilder sb,
        char quoteChar = '\'',
        char separator = DefaultSeparator)
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

    public static Vocab? Parse(string text, char separator = DefaultSeparator)
    {
        string[] entries = text.Split(separator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
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
    public static IEnumerable<string> Strings(this IVocab vocab)
    {
        return from entry in vocab
               select entry.Text;
    }
}
