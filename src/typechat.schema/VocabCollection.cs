// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

/// <summary>
/// An in-memory vocabulary collection
/// </summary>
public class VocabCollection : IVocabCollection
{
    private Dictionary<string, NamedVocab> _vocabs = new();

    /// <summary>
    /// Create a new vocabulary collection
    /// </summary>
    public VocabCollection()
    {
    }

    /// <summary>
    /// Create a new vocabulary collection with a set of vocab records
    /// </summary>
    internal VocabCollection(IDictionary<string, string[]> vocabRecords)
    {
        ArgumentVerify.ThrowIfNull(vocabRecords, nameof(vocabRecords));
        foreach (var record in vocabRecords)
        {
            var vocab = new Vocab(record.Value);
            vocab.TrimExcess();
            Add(record.Key, vocab);
        }
    }

    /// <summary>
    /// Count of vocabs in this collection
    /// </summary>
    public int Count => _vocabs.Count;

    /// <summary>
    /// Add a new vocabulary
    /// </summary>
    /// <param name="vocab"></param>
    public void Add(NamedVocab vocab)
    {
        ArgumentVerify.ThrowIfNull(vocab, nameof(vocab));
        _vocabs.Add(vocab.Name, vocab);
    }

    /// <summary>
    /// Add a named vocab
    /// </summary>
    /// <param name="name"></param>
    /// <param name="vocab"></param>
    public void Add(string name, IVocab vocab)
        => Add(new NamedVocab(name, vocab));

    /// <summary>
    /// Add a named vocab. The vocab text is dynamically parsed using Vocab.Parse
    /// </summary>
    /// <param name="name"></param>
    /// <param name="vocabText">Text containing vocab entries that are parsed</param>
    public void Add(string name, string vocabText)
        => Add(name, Vocab.Parse(vocabText));

    public void Clear() => _vocabs.Clear();

    /// <summary>
    /// Does a vocabulary with the given name exist?
    /// </summary>
    /// <param name="vocabName"></param>
    /// <returns></returns>
    public bool Contains(string vocabName) => _vocabs.ContainsKey(vocabName);

    /// <summary>
    /// Does the vocabulary contain the given item
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool Contains(NamedVocab item) => _vocabs.ContainsKey(item.Name);

    public NamedVocab? Get(string name)
    {
        return _vocabs.TryGetValue(name, out NamedVocab vocab) ? vocab : null;
    }

    public bool Remove(NamedVocab item)
    {
        ArgumentVerify.ThrowIfNull(item, nameof(item));
        return _vocabs.Remove(item.Name);
    }

    public IEnumerator<NamedVocab> GetEnumerator()
        => _vocabs.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}

public static class VocabCollectionEx
{
    public static bool Contains(this IVocabCollection vocabs, string vocabName, VocabEntry entry)
    {
        NamedVocab? vocabType = vocabs.Get(vocabName);
        return vocabType is not null && vocabType.Vocab.Contains(entry);
    }
}
