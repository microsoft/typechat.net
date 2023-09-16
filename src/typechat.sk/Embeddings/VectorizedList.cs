// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Embeddings;

/// <summary>
/// A List of items with associated embeddings
/// You can use this to build collections of objects that you can search using semantic similarity
/// </summary>
/// <typeparam name="T"></typeparam>
public class VectorizedList<T> : IEnumerable<KeyValuePair<T, Embedding>>
{
    List<T> _buffer;
    List<Embedding> _embeddings;

    [JsonConstructor]
    public VectorizedList()
        : this(8)
    { }
    /// <summary>
    /// Create a new VectorizedList
    /// </summary>
    /// <param name="capacity">default capacity is 8</param>
    public VectorizedList(int capacity)
    {
        _buffer = new List<T>(capacity);
        _embeddings = new List<Embedding>(capacity);
    }

    /// <summary>
    /// Retrieve the item at index
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public T this[int index] => _buffer[index];
    /// <summary>
    /// Number of items in this list
    /// </summary>
    public int Count => _buffer.Count;
    /// <summary>
    /// Is the collection empty
    /// </summary>
    public bool IsEmpty => Count <= 0;
    /// <summary>
    /// Enumerate all items
    /// </summary>
    public IEnumerable<T> AllItems => _buffer;
    /// <summary>
    /// Add an item and its embedding to the collection
    /// </summary>
    /// <param name="item">item</param>
    /// <param name="embedding">embedding for the item</param>
    public void Add(T item, Embedding embedding)
    {
        _buffer.Add(item);
        _embeddings.Add(embedding);
    }
    /// <summary>
    /// Add an item and its embedding to the collection
    /// </summary>
    /// <param name="item"></param>
    public void Add(KeyValuePair<T, Embedding> item)
    {
        _buffer.Add(item.Key);
        _embeddings.Add(item.Value);
    }

    /// <summary>
    /// Put an item and its embedding at the given position in the collection
    /// </summary>
    /// <param name="index">position</param>
    /// <param name="item">item to put</param>
    /// <param name="embedding">embedding for the item</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void PutAt(int index, T item, Embedding embedding)
    {
        if (index >= _buffer.Count)
        {
            throw new ArgumentOutOfRangeException();
        }
        _buffer[index] = item;
        _embeddings[index] = embedding;
    }

    /// <summary>
    /// Does the item exist in this collection
    /// </summary>
    /// <param name="item">item to search for</param>
    /// <returns>true if found</returns>
    public bool Contains(T item) => IndexOf(item) >= 0;

    /// <summary>
    /// Get the position of the given item in this collection
    /// </summary>
    /// <param name="item">item to locate</param>
    /// <returns>-1 if not found</returns>
    public int IndexOf(T item)
    {
        for (int i = 0, count = _buffer.Count; i < count; ++i)
        {
            if (_buffer[i].Equals(item))
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Insert an item and its embedding
    /// </summary>
    /// <param name="index">index</param>
    /// <param name="item">item</param>
    /// <param name="embedding">embedding</param>
    public void InsertAt(int index, T item, Embedding embedding)
    {
        _buffer.Insert(index, item);
        _embeddings.Insert(index, embedding);
    }

    /// <summary>
    /// Remove an item
    /// </summary>
    /// <param name="item">item to remove</param>
    /// <returns></returns>
    public bool Remove(T item)
    {
        int i = IndexOf(item);
        if (i >= 0)
        {
            RemoveAt(i);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Remove the item at this index
    /// </summary>
    /// <param name="index">item to remove</param>
    public void RemoveAt(int index)
    {
        _buffer.RemoveAt(index);
        _embeddings.RemoveAt(index);
    }

    /// <summary>
    /// Clear the collection
    /// </summary>
    public void Clear()
    {
        _buffer.Clear();
        _embeddings.Clear();
    }

    /// <summary>
    /// Trim the collection
    /// </summary>
    public void Trim()
    {
        _buffer.TrimExcess();
        _embeddings.TrimExcess();
    }

    /// <summary>
    /// Return the position of the nearest item
    /// </summary>
    /// <param name="other"></param>
    /// <returns>index of nearest item</returns>
    public int IndexOfNearest(Embedding other)
    {
        return _embeddings.Nearest(other).Value;
    }

    /// <summary>
    /// Return the item nearest to the given embedding
    /// </summary>
    /// <param name="other"></param>
    /// <returns>nearest item, or default</returns>
    public T Nearest(Embedding other)
    {
        int i = IndexOfNearest(other);
        if (i >= 0)
        {
            return this[i];
        }
        return default;
    }

    /// <summary>
    /// Return the top N nearest matches
    /// </summary>
    /// <param name="other">embedding to match against</param>
    /// <param name="matches">matches collection</param>
    /// <returns></returns>
    public TopNCollection<int> IndexOfNearest(Embedding other, TopNCollection<int> matches)
    {
        return _embeddings.Nearest(other, matches);
    }

    /// <summary>
    /// Return the topN nearest matches nearest to the given embedding
    /// </summary>
    /// <param name="other"></param>
    /// <param name="topNCount"></param>
    /// <returns>An enumeration of top matches</returns>
    public IEnumerable<T> Nearest(Embedding other, int topNCount)
    {
        TopNCollection<int> matches = new TopNCollection<int>(topNCount);
        IndexOfNearest(other, matches);
        matches.Sort();
        for (int i = 0; i < matches.Count; ++i)
        {
            yield return this[matches[i].Value];
        }
    }

    public IEnumerator<KeyValuePair<T, Embedding>> GetEnumerator()
    {
        for (int i = 0, count = _buffer.Count; i < count; ++i)
        {
            yield return new KeyValuePair<T, Embedding>(_buffer[i], _embeddings[i]);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
