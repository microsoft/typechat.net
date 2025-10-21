// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel.Embeddings;
using Microsoft.TypeChat.Embeddings;

namespace Microsoft.TypeChat.SemanticKernel.Embeddings;

public static class EmbeddingsEx
{
    /// <summary>
    /// Generate an embedding using the given model
    /// </summary>
    /// <param name="model">model to use</param>
    /// <param name="text">text for which to generate an embedding</param>
    /// <param name="kernel">kernel to use</param>
    /// <param name="cancelToken">optional cancel token</param>
    /// <returns></returns>
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    public static async Task<Embedding> GenerateEmbeddingAsync(this ITextEmbeddingGenerationService model, string text, Kernel kernel, CancellationToken cancelToken = default)
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    {
        string[] texts = new string[] { text };
        var results = await model.GenerateEmbeddingsAsync(texts, kernel, cancelToken).ConfigureAwait(false);
        return new Embedding(results[0]);
    }
}
