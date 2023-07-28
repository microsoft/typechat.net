// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class VocabAttribute : Attribute
{
    public VocabAttribute() { }
    public VocabAttribute(string vocabName)
        : this(vocabName, null)
    {
    }
    public VocabAttribute(string vocabName, string entries)
    {
        ArgumentException.ThrowIfNullOrEmpty(vocabName, nameof(vocabName));
        Name = vocabName;
        Entries = entries;
    }

    public string? Name { get; set; }
    public bool Inline { get; set; } = true;
    public string? Entries { get; set; }

    public bool HasName => !string.IsNullOrEmpty(Name);
    public bool HasEntries => !string.IsNullOrEmpty(Entries);

    public Vocab? ToVocab()
    {
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
