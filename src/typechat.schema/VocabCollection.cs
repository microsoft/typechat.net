// Copyright (c) Microsoft. All rights reserved.

using System.Collections;

namespace Microsoft.TypeChat.Schema;

public interface IVocabCollection : IEnumerable<VocabType>
{
    void Add(VocabType vocab);
    VocabType? Get(string name);
}

public class VocabCollection : IVocabCollection
{
    Dictionary<string, VocabType> _vocabs;

    public VocabCollection()
    {
        _vocabs = new Dictionary<string, VocabType>();
    }

    public int Count => _vocabs.Count;

    public void Add(VocabType vocab)
    {
        ArgumentNullException.ThrowIfNull(vocab, nameof(vocab));
        _vocabs.Add(vocab.Name, vocab);
    }

    public void Add(string name, IVocab vocab)
    {
        Add(new VocabType(name, vocab));
    }

    public void Clear() => _vocabs.Clear();

    public bool Contains(VocabType item) => _vocabs.ContainsKey(item.Name);

    public VocabType? Get(string name)
    {
        return _vocabs.GetValueOrDefault(name, null);
    }

    public IEnumerator<VocabType> GetEnumerator()
    {
        return _vocabs.Values.GetEnumerator();
    }

    public bool Remove(VocabType item)
    {
        ArgumentNullException.ThrowIfNull(item, nameof(item));
        return _vocabs.Remove(item.Name);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public static class VocabStoreEx
{
    public static VocabType? VocabFor(this IVocabCollection store, MemberInfo member)
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
