// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Embeddings;

/// <summary>
/// An in-memory vector text table that automatically vectorizes given items using a model
/// All embeddings are normalized automatically for performance. With all embeddings of unit length,
/// dot products can replace cosine similarity
/// </summary>
/// <typeparam name="T"></typeparam>
public class VectorTextIndex<T>
{
    TextEmbeddingModel _model;
    VectorizedList<T> _list;

    /// <summary>
    /// Create a new VectorTextIndex
    /// </summary>
    /// <param name="model"></param>
    /// <param name="capacity"></param>
    public VectorTextIndex(TextEmbeddingModel model, int capacity = 4)
    {
        ArgumentVerify.ThrowIfNull(model, nameof(model));
        _model = model;
        _list = new VectorizedList<T>(capacity);
    }

    /// <summary>
    /// Count of items in the index
    /// </summary>
    public int Count => _list.Count;

    /// <summary>
    /// Return the item at the given position
    /// </summary>
    /// <param name="index"></param>
    /// <returns>item</returns>
    public T this[int index] => _list[index];

    /// <summary>
    /// Add an item to the collection. Its associated textKey will be vectorized into an embedding
    /// </summary>
    /// <param name="item">item to add to the index</param>
    /// <param name="textRepresentation">The text representation of the item; its transformed into an embedding</param>
    /// <param name="cancelToken">cancel token</param>
    /// <returns>the position of the embedding in the vector index</returns>
    public async Task<int> AddAsync(T item, string textRepresentation, CancellationToken cancelToken = default)
    {
        ArgumentVerify.ThrowIfNullOrEmpty(textRepresentation, nameof(textRepresentation));

        var embedding = await GetNormalizedEmbeddingAsync(textRepresentation, cancelToken).ConfigureAwait(false);
        int i = _list.Count;
        _list.Add(item, embedding);
        return i;
    }

    /// <summary>
    /// Remove the item at the given index
    /// </summary>
    /// <param name="index">index to remove at</param>
    public void RemoveAt(int index)
    {
        _list.RemoveAt(index);
    }

    /// <summary>
    /// Find nearest match to the given text
    /// </summary>
    /// <param name="text"></param>
    /// <returns>nearest item</returns>
    public async Task<T> NearestAsync(string text, CancellationToken cancelToken = default)
    {
        var embedding = await GetNormalizedEmbeddingAsync(text, cancelToken).ConfigureAwait(false);
        return _list.Nearest(embedding, EmbeddingDistance.Dot);
    }

    /// <summary>
    /// Return topN text from the collection closest to the given text
    /// </summary>
    /// <param name="text">text to search for</param>
    /// <param name="maxMatches">max matches</param>
    /// <param name="cancelToken">optional cancel token</param>
    /// <returns>list of matches</returns>
    public async Task<List<T>> NearestAsync(string text, int maxMatches, CancellationToken cancelToken = default)
    {
        var embedding = await GetNormalizedEmbeddingAsync(text, cancelToken).ConfigureAwait(false);
        return _list.Nearest(embedding, maxMatches, EmbeddingDistance.Dot).ToList();
    }

    /// <summary>
    /// Return the positions of the nearest matches
    /// </summary>
    /// <param name="text">find nearest matches to this text</param>
    /// <param name="maxMatches">max number of matches</param>
    /// <param name="cancelToken">optional cancel token</param>
    /// <returns>matches</returns>
    public Task<TopNCollection<int>> IndexOfNearestAsync(string text, int maxMatches, CancellationToken cancelToken = default)
    {
        TopNCollection<int> matches = new TopNCollection<int>(maxMatches);
        return IndexOfNearestAsync(text, maxMatches, cancelToken);
    }
    /// <summary>
    /// Return the positions of the nearest matches
    /// </summary>
    /// <param name="text">find nearest matches to this text</param>
    /// <param name="matches">matches collection</param>
    /// <param name="cancelToken">optional cancel token</param>
    /// <returns>matches</returns>
    public async Task<TopNCollection<int>> IndexOfNearestAsync(string text, TopNCollection<int> matches = null, CancellationToken cancelToken = default)
    {
        var embedding = await GetNormalizedEmbeddingAsync(text, cancelToken).ConfigureAwait(false);
        return _list.IndexOfNearest(embedding, matches, EmbeddingDistance.Dot);
    }

    async Task<Embedding> GetNormalizedEmbeddingAsync(string text, CancellationToken cancelToken)
    {
        var embedding = await _model.GenerateEmbeddingAsync(text, cancelToken).ConfigureAwait(false);
        embedding.NormalizeInPlace();
        return embedding;
    }
}
