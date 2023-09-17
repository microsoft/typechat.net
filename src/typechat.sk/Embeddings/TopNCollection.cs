// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Embeddings;
/// <summary>
/// A collector for Top N matches. Keeps only the best N matches by Score.
/// Automatically flushes out any not in the top N.
/// By default, items are not sorted by score until you call <see cref="TopNCollection{T}.Sort"/>.
/// </summary>
public class TopNCollection<T>
{
    MinHeap<T> _heap;
    /// <summary>
    /// Initializes a new instance of the <see cref="TopNCollection{T}"/> class.
    /// </summary>
    /// <param name="topNCount">The maximum number of matches.</param>
    public TopNCollection(int topNCount)
    {
        this.MaxItems = topNCount;
        this._heap = new MinHeap<T>(topNCount);
    }

    public int Count => _heap.Count;
    public ScoredValue<T> this[int index] => _heap[index];

    /// <summary>
    /// The maximum number of items allowed in the collection.
    /// </summary>
    public int MaxItems { get; set; }

    /// <summary>
    /// Resets the collection, allowing it to be reused.
    /// </summary>
    public void Clear()
    {
        _heap.Clear();
    }

    /// <summary>
    /// Adds a single scored value to the collection.
    /// </summary>
    /// <param name="value">The scored value to add.</param>
    public void Add(ScoredValue<T> value)
    {
        Add(value.Value, value.Score);
    }

    /// <summary>
    /// Adds a value with a specified score to the collection.
    /// </summary>
    /// <param name="value">The value to add.</param>
    /// <param name="score">The score associated with the value.</param>
    public void Add(T value, double score)
    {
        if (_heap.Count == MaxItems)
        {
            // Queue is full. We will need to dequeue the item with lowest weight
            if (score <= _heap.Top.Score)
            {
                // This score is lower than the lowest score on the queue right now. Ignore it
                return;
            }

            _heap.RemoveTop();
        }

        _heap.Add(new ScoredValue<T>(value, score));
    }

    /// <summary>
    /// Sorts the collection in descending order by score.
    /// </summary>
    public void Sort()
    {
        _heap.SortDescending();
    }
}
