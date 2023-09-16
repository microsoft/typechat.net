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
    public static async Task<Embedding> GenerateEmbeddingAsync(this IEmbeddingGeneration<string, float> model, string text, CancellationToken cancelToken = default)
    {
        Embedding embedding = await model.GenerateEmbeddingAsync(text, cancelToken).ConfigureAwait(false);
        return embedding;
    }

    public static ScoredValue<int> Nearest(this IList<Embedding> list, Embedding other)
    {
        ScoredValue<int> best = new ScoredValue<int>(-1, double.MinValue);
        for (int i = 0; i < list.Count; ++i)
        {
            double score = list[i].Similarity(other);
            if (score > best.Score)
            {
                best = new ScoredValue<int>(i, score);
            }
        }
        return best;
    }

    public static TopNCollection<int> Nearest(this IList<Embedding> list, Embedding other, TopNCollection<int> matches)
    {
        for (int i = 0; i < list.Count; ++i)
        {
            double score = list[i].Similarity(other);
            matches.Add(i, score);
        }
        return matches;
    }

}
