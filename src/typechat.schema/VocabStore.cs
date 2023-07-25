// Copyright (c) Microsoft. All rights reserved.

using System.Collections;

namespace Microsoft.TypeChat.Schema;

public interface IVocabStore : IEnumerable<VocabType>
{
    VocabType? Get(string name);
}

public class VocabStore : IVocabStore
{
    Dictionary<string, VocabType> _vocabs;

    public VocabStore()
    {
        _vocabs = new Dictionary<string, VocabType>();
    }

    public void Add(VocabType vocab)
    {
        ArgumentNullException.ThrowIfNull(vocab, nameof(vocab));
        _vocabs.Add(vocab.Name, vocab);
    }

    public void Add(string name, IVocab vocab)
    {
        Add(new VocabType(name, vocab));
    }

    public VocabType? Get(string name)
    {
        return _vocabs.GetValueOrDefault(name, null);
    }

    public IEnumerator<VocabType> GetEnumerator()
    {
        return _vocabs.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public static class VocabStoreEx
{
    public static VocabType? VocabFor(this IVocabStore store, MemberInfo member)
    {
        ArgumentNullException.ThrowIfNull(member, nameof(member));

        string? vocabName = member.VocabName();
        if (string.IsNullOrEmpty(vocabName))
        {
            return null;
        }

        return store.Get(vocabName);
    }
}
