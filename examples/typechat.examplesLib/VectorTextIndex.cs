// Copyright (c) Microsoft. All rights reserved.

using Microsoft.TypeChat.Embeddings;

namespace Microsoft.TypeChat;

/// <summary>
/// VectorTextIndex is an in-memory vector text index that automatically vectorizes given items using a model
/// All embeddings are normalized automatically for performance.
/// Each item T has an associated text description. It is this description that is indexed using embeddings.
/// 
/// The VectorTextIndex is also a TextRequestRouter that uses embeddings to route text requests
/// </summary>
/// <typeparam name="T"></typeparam>
public class VectorTextIndex<T> : ITextRequestRouter<T>
{
    private TextEmbeddingModel _model;
    private VectorizedList<T> _list;

    /// <summary>
    /// Create a new VectorTextIndex
    /// </summary>
    /// <param name="model">embedding model</param>
    public VectorTextIndex(TextEmbeddingModel model)
        : this(model, new VectorizedList<T>())
    {
    }

    /// <summary>
    /// Create a new VectorTextIndex
    /// </summary>
    /// <param name="model">model to use</param>
    /// <param name="list">vector list to use</param>
    public VectorTextIndex(TextEmbeddingModel model, VectorizedList<T> list)
    {
        ArgumentVerify.ThrowIfNull(model, nameof(model));
        ArgumentVerify.ThrowIfNull(list, nameof(list));
        _model = model;
        _list = list;
    }

    /// <summary>
    /// Items in this index
    /// </summary>
    public VectorizedList<T> Items => _list;

    /// <summary>
    /// Route the given request to the semantically nearest T
    /// Does so by comparing the embedding of request to that of all registered T
    /// </summary>
    /// <param name="request">tequest</param>
    /// <param name="cancellationToken">optional cancel token</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task<T> RouteRequestAsync(string request, CancellationToken cancellationToken = default)
        => NearestAsync(request, cancellationToken);

    /// <summary>
    /// Add an item to the collection. Its associated textKey will be vectorized into an embedding
    /// </summary>
    /// <param name="item">item to add to the index</param>
    /// <param name="textRepresentation">The text representation of the item; its transformed into an embedding</param>
    /// <param name="cancellationToken">cancel token</param>
    public async Task AddAsync(T item, string textRepresentation, CancellationToken cancellationToken = default)
    {
        ArgumentVerify.ThrowIfNullOrEmpty(textRepresentation, nameof(textRepresentation));

        var embedding = await GetNormalizedEmbeddingAsync(textRepresentation, cancellationToken).ConfigureAwait(false);
        _list.Add(item, embedding);
    }

    /// <summary>
    /// A multiple items to the collection.
    /// If the associated embedding model supports batching, this can be much faster
    /// </summary>
    /// <param name="items">items to add to the collection</param>
    /// <param name="textRepresentations">the text representations of these items</param>
    /// <param name="cancellationToken">optional cancel token</param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task AddAsync(T[] items, string[] textRepresentations, CancellationToken cancellationToken = default)
    {
        ArgumentVerify.ThrowIfNull(items, nameof(items));
        ArgumentVerify.ThrowIfNull(textRepresentations, nameof(textRepresentations));

        if (items.Length != textRepresentations.Length)
        {
            throw new ArgumentException("items and their representations must of the same length");
        }

        Embedding[] embeddings = await GetNormalizedEmbeddingAsync(textRepresentations, cancellationToken).ConfigureAwait(false);
        if (embeddings.Length != items.Length)
        {
            throw new InvalidOperationException($"Embedding length {embeddings.Length} does not match items length {items.Length}");
        }

        for (int i = 0; i < items.Length; ++i)
        {
            _list.Add(items[i], embeddings[i]);
        }
    }

    /// <summary>
    /// Find nearest match to the given text
    /// </summary>
    /// <param name="text"></param>
    /// <param name="cancellationToken">optional cancel token</param>
    /// <returns>nearest item</returns>
    public async Task<T> NearestAsync(string text, CancellationToken cancellationToken = default)
    {
        var embedding = await GetNormalizedEmbeddingAsync(text, cancellationToken).ConfigureAwait(false);
        return _list.Nearest(embedding, EmbeddingDistance.Dot);
    }

    /// <summary>
    /// Return topN text from the collection closest to the given text
    /// </summary>
    /// <param name="text">text to search for</param>
    /// <param name="maxMatches">max matches</param>
    /// <param name="cancellationToken">optional cancel token</param>
    /// <returns>list of matches</returns>
    public async Task<List<T>> NearestAsync(string text, int maxMatches, CancellationToken cancellationToken = default)
    {
        var embedding = await GetNormalizedEmbeddingAsync(text, cancellationToken).ConfigureAwait(false);
        return _list.Nearest(embedding, maxMatches, EmbeddingDistance.Dot).ToList();
    }

    private async Task<Embedding> GetNormalizedEmbeddingAsync(string text, CancellationToken cancellationToken)
    {
        var embedding = await _model.GenerateEmbeddingAsync(text, cancellationToken).ConfigureAwait(false);
        embedding.NormalizeInPlace();
        return embedding;
    }

    private async Task<Embedding[]> GetNormalizedEmbeddingAsync(string[] texts, CancellationToken cancellationToken)
    {
        var embeddings = await _model.GenerateEmbeddingsAsync(texts, cancellationToken).ConfigureAwait(false);
        for (int i = 0; i < embeddings.Length; ++i)
        {
            embeddings[i].NormalizeInPlace();
        }

        return embeddings;
    }
}
