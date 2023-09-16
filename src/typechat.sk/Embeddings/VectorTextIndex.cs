// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Embeddings;

public interface ITextIndex<K>
{
    Task<IList<K>> MatchAsync(string text, int maxMatches, CancellationToken cancelToken);
}

/// <summary>
/// A Vector Text Index that automatically vectorizes given items using a model
/// It does the same for retriefval
/// </summary>
/// <typeparam name="T"></typeparam>
public class VectorTextIndex<T> : VectorizedList<T>
{
    ITextEmbeddingGeneration _model;
    VectorizedList<T> _list;

    public VectorTextIndex(ITextEmbeddingGeneration model, int capacity = 8)
        : base(capacity)
    {
        ArgumentVerify.ThrowIfNull(model, nameof(model));

        _model = model;
        _list = new VectorizedList<T>();
    }

    /// <summary>
    /// Add an item to the collection. Its associated textKey will be vectorized into an embedding
    /// </summary>
    /// <param name="item"></param>
    /// <param name="textKey"></param>
    /// <returns></returns>
    public async Task AddAsync(T item, string textKey)
    {
        var embedding = await _model.GenerateEmbeddingAsync(textKey).ConfigureAwait(false);
        Add(item, embedding);
    }

    /// <summary>
    /// Find nearest match to the given text
    /// </summary>
    /// <param name="text"></param>
    /// <returns>nearest item</returns>
    public async Task<T> NearestAsync(string text)
    {
        var embedding = await _model.GenerateEmbeddingAsync(text).ConfigureAwait(false);
        return Nearest(embedding);
    }

    /// <summary>
    /// Return topN text from the collection closest to the given text
    /// </summary>
    /// <param name="text">text to search for</param>
    /// <param name="topNCount">max matches</param>
    /// <param name="cancelToken">optional cancel token</param>
    /// <returns></returns>
    public async Task<List<T>> NearestAsync(string text, int topNCount, CancellationToken cancelToken = default)
    {
        var embedding = await _model.GenerateEmbeddingAsync(text, cancelToken).ConfigureAwait(false);
        return Nearest(embedding, topNCount).ToList();
    }

    /// <summary>
    /// Return the positions of the nearest matches
    /// </summary>
    /// <param name="text">find nearest matches to this text</param>
    /// <param name="matches">matches collection</param>
    /// <param name="cancelToken">optional cancel token</param>
    /// <returns></returns>
    public async Task<TopNCollection<int>> IndexOfNearestAsync(string text, TopNCollection<int> matches, CancellationToken cancelToken = default)
    {
        var embedding = await _model.GenerateEmbeddingAsync(text, cancelToken).ConfigureAwait(false);
        return IndexOfNearest(embedding, matches);
    }
}
