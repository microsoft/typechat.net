// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

public interface IVocab : IEnumerable<VocabEntry>
{
    bool Contains(VocabEntry entry);
}

public class Vocab : SortedSet<VocabEntry>, IVocab
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
}
