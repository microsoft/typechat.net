// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel.Embeddings;
using Microsoft.TypeChat.Embeddings;
using Microsoft.TypeChat.SemanticKernel.Embeddings;

namespace Microsoft.TypeChat;

public class TextEmbeddingModel
{
    private readonly Kernel _kernel;
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    private readonly ITextEmbeddingGenerationService _model;
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    private readonly ModelInfo _modelInfo;

    /// <summary>
    /// Create a new text embedding model from the OpenAI config
    /// By default, uses model in config.Model
    /// </summary>
    /// <param name="config">OpenAI configuration</param>
    /// <param name="modelInfo">information about the model to create</param>
    public TextEmbeddingModel(OpenAIConfig config, ModelInfo? modelInfo = null)
    {
        ArgumentVerify.ThrowIfNull(config, nameof(config));
        config.Validate();

        IKernelBuilder kb = Kernel.CreateBuilder();
        kb.WithEmbeddingModel(config.Model, config);

        _kernel = kb.Build();
        modelInfo ??= config.Model;
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        _model = _kernel.GetRequiredService<ITextEmbeddingGenerationService>(modelInfo.Name);
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        _modelInfo = modelInfo;
    }

    /// <summary>
    /// Create a new text embedding model that uses the given embedding generation service
    /// </summary>
    /// <param name="service"></param>
    /// <param name="modelInfo">information about the model to create</param>
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    public TextEmbeddingModel(ITextEmbeddingGenerationService service, ModelInfo? modelInfo)
    {
        ArgumentVerify.ThrowIfNull(service, nameof(service));
        _model = service;
        _modelInfo = modelInfo;
    }
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

    /// <summary>
    /// Information about the model
    /// </summary>
    public ModelInfo ModelInfo => _modelInfo;

    /// <summary>
    /// Generate an embedding
    /// </summary>
    /// <param name="text">text to create embeddings for</param>
    /// <param name="cancelToken">optional cancel token</param>
    /// <returns></returns>
    public Task<Embedding> GenerateEmbeddingAsync(string text, CancellationToken cancelToken = default)
    {
        return _model.GenerateEmbeddingAsync(text, _kernel, cancelToken);
    }

    /// <summary>
    /// Generate embeddings in a batch
    /// </summary>
    /// <param name="texts">list of texts to turn into embeddings</param>
    /// <param name="cancelToken">optional cancel token</param>
    /// <returns></returns>
    public async Task<Embedding[]> GenerateEmbeddingsAsync(IList<string> texts, CancellationToken cancelToken = default)
    {
        var results = await _model.GenerateEmbeddingsAsync(texts, _kernel, cancelToken);
        Embedding[] embeddings = new Embedding[results.Count];
        for (int i = 0; i < embeddings.Length; ++i)
        {
            embeddings[i] = new Embedding(results[i]);
        }
        return embeddings;
    }
}
