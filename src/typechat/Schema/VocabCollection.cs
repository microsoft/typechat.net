// Copyright (c) Microsoft. All rights reserved.

using System.Collections;

namespace Microsoft.TypeChat.Schema;

/// <summary>
/// A collection of named vocabularies.
/// Vocabulary collections can be dynamic and load vocabularies at runtime from data bases, files and so on
/// </summary>
public interface IVocabCollection : IEnumerable<NamedVocab>
{
    NamedVocab? Get(string name);
}

/// <summary>
/// An in-memory vocabulary collection
/// </summary>
public class VocabCollection : IVocabCollection
{
    Dictionary<string, NamedVocab> _vocabs;

    /// <summary>
    /// Create a new vocabulary collection
    /// </summary>
    public VocabCollection()
    {
        _vocabs = new Dictionary<string, NamedVocab>();
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
        ArgumentNullException.ThrowIfNull(vocab, nameof(vocab));
        _vocabs.Add(vocab.Name, vocab);
    }
    /// <summary>
    /// Add a named vocab
    /// </summary>
    /// <param name="name"></param>
    /// <param name="vocab"></param>
    public void Add(string name, IVocab vocab)
    {
        Add(new NamedVocab(name, vocab));
    }
    /// <summary>
    /// Add a named vocab. The vocab text is dynamically parsed using Vocab.Parse
    /// </summary>
    /// <param name="name"></param>
    /// <param name="vocabText">Text containing vocab entries that are parsed</param>
    public void Add(string name, string vocabText)
    {
        Add(name, Vocab.Parse(vocabText));
    }

    public void Clear() => _vocabs.Clear();

    /// <summary>
    /// Does a vocabulary with the given name exist?
    /// </summary>
    /// <param name="vocabName"></param>
    /// <returns></returns>
    public bool Contains(string vocabName) => _vocabs.ContainsKey(vocabName);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool Contains(NamedVocab item) => _vocabs.ContainsKey(item.Name);

    public NamedVocab? Get(string name)
    {
        return _vocabs.GetValueOrDefault(name, null);
    }

    public IEnumerator<NamedVocab> GetEnumerator()
    {
        return _vocabs.Values.GetEnumerator();
    }

    public bool Remove(NamedVocab item)
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
        NamedVocab? vocabType = vocabs.Get(vocabName);
        return vocabType != null && vocabType.Vocab.Contains(entry);
    }
}

