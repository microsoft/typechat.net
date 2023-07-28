// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class VocabAttribute : Attribute
{
    public VocabAttribute() { }

    public VocabAttribute(string entries, string? vocabName = null)
    {
        Entries = entries;
        Name = vocabName;
    }

    public string? Name { get; set; }
    public bool Inline { get; set; } = true;
    public string? Entries { get; set; }

    public bool HasName => !string.IsNullOrEmpty(Name);
    public bool HasEntries => !string.IsNullOrEmpty(Entries);

    public Vocab? ToVocab()
    {
        if (!HasEntries)
        {
            return null;
        }
        return Vocab.Parse(Entries);
    }

    public VocabType? ToVocabType()
    {
        if (!HasEntries)
        {
            return null;
        }
        Vocab? vocab = ToVocab();
        if (vocab == null)
        {
            return null;
        }
        return new VocabType(Name, vocab);
    }
}
