// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

public interface IVocab : IEnumerable<VocabEntry>
{
    bool Contains(VocabEntry entry);
}

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
}

public static class VocabEx
{
    public static IEnumerable<string> Strings(this IVocab vocab)
    {
        return from entry in vocab
               select entry.Text;
    }
}
