// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Embeddings;

public static class EmbeddingsEx
{
    /// <summary>
    /// Generate an embedding using the given model
    /// </summary>
    /// <param name="model">model to use</param>
    /// <param name="text">text for which to generate an embedding</param>
    /// <param name="cancelToken">optional cancel token</param>
    /// <returns></returns>
    public static async Task<Embedding> GenerateEmbeddingAsync(this ITextEmbeddingGeneration model, string text, CancellationToken cancelToken = default)
    {
        string[] texts = new string[] { text };
        var results = await model.GenerateEmbeddingsAsync(texts, cancelToken).ConfigureAwait(false);
        return new Embedding(results[0]);
    }

    /// <summary>
    /// Given a list of embedings, return the index of the item that is nearest to 'other'
    /// Return
    /// </summary>
    /// <param name="list">list of candidate embeddings</param>
    /// <param name="embedding">embedding to compare against</param>
    /// <param name="distanceType">distance measure to use</param>
    /// <returns>The index of the nearest neighbor</returns>
    public static ScoredValue<int> IndexOfNearest(this IList<Embedding> list, Embedding embedding, EmbeddingDistance distanceType)
    {
        ScoredValue<int> best = new ScoredValue<int>(-1, double.MinValue);
        for (int i = 0; i < list.Count; ++i)
        {
            Score score = list[i].Similarity(embedding, distanceType);
            if (score > best.Score)
            {
                best = new ScoredValue<int>(i, score);
            }
        }

        return best;
    }

    /// <summary>
    /// Return indexes of the nearest neighbors of the given embedding
    /// </summary>
    /// <param name="list">list of candidate embeddings</param>
    /// <param name="embedding">embedding to compare against</param>
    /// <param name="matches">match collector</param>
    /// <param name="distanceType">distance measure to use</param>
    /// <returns>matches</returns>
    public static TopNCollection<int> Nearest(this IList<Embedding> list, Embedding embedding, TopNCollection<int> matches, EmbeddingDistance distanceType)
    {
        for (int i = 0; i < list.Count; ++i)
        {
            double score = list[i].Similarity(embedding, distanceType);
            matches.Add(i, score);
        }

        return matches;
    }

    /// <summary>
    /// Return indexes of the nearest neighbors of the given embedding
    /// </summary>
    /// <param name="list">list of candidate embeddings</param>
    /// <param name="embedding">embedding to compare against</param>
    /// <param name="maxMatches">max matches</param>
    /// <param name="distanceType">distance measure</param>
    /// <returns>matches</returns>
    public static TopNCollection<int> Nearest(this IList<Embedding> list, Embedding embedding, int maxMatches, EmbeddingDistance distanceType)
    {
        TopNCollection<int> matches = new TopNCollection<int>(maxMatches);
        list.Nearest(embedding, matches, distanceType);
        matches.Sort();
        return matches;
    }
}
