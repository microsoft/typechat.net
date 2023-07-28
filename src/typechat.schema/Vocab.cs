// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

/// <summary>
/// A Vocabulary is a dynamic string table whose values ARE NOT HARDCODED IN CODE - e.g. lists of product names
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
    public Vocab() { }

    public Vocab(params string[] entries)
    {
        for (int i = 0; i < entries.Length; ++i)
        {
            Add(entries[i]);
        }
    }

    public static implicit operator Vocab(string[] values)
    {
        return new Vocab(values);
    }

    public static Vocab? Parse(string text, char seperator = '|')
    {
        string[] entries = text.Split(seperator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (entries == null || entries.Length == 0)
        {
            return null;
        }
        return new Vocab(entries);
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
