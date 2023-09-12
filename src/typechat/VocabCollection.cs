// Copyright (c) Microsoft. All rights reserved.

using System.Collections;

namespace Microsoft.TypeChat;

public interface IVocabCollection : IEnumerable<VocabType>
{
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

    public void Add(string name, string vocabText)
    {
        Add(name, Vocab.Parse(vocabText));
    }

    public void Add(IEnumerable<VocabType> vocabs)
    {
        ArgumentNullException.ThrowIfNull(vocabs, nameof(vocabs));

        foreach (var vocab in vocabs)
        {
            Add(vocab);
        }
    }

    public void Add(IDictionary<string, string[]> vocabRecords)
    {
        ArgumentNullException.ThrowIfNull(vocabRecords, nameof(vocabRecords));
        foreach (var record in vocabRecords)
        {
            var vocab = new Vocab(record.Value);
            vocab.TrimExcess();
            Add(record.Key, vocab);
        }
    }

    public void Clear() => _vocabs.Clear();

    public bool Contains(string vocabName) => _vocabs.ContainsKey(vocabName);
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

public static class VocabCollectionEx
{
    public static bool Contains(this IVocabCollection vocabs, string vocabName, VocabEntry entry)
    {
        VocabType? vocabType = vocabs.Get(vocabName);
        return (vocabType != null && vocabType.Vocab.Contains(entry));
    }
}

