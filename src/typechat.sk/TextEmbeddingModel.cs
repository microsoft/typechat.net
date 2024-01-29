// Copyright (c) Microsoft. All rights reserved.

using Microsoft.TypeChat.Embeddings;

namespace Microsoft.TypeChat;

public class TextEmbeddingModel
{
    Kernel _kernel;
    ITextEmbeddingGenerationService _model;
    ModelInfo _modelInfo;

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
        kb.WithEmbeddingModel(config.Model, config)
          .WithRetry(config);

        _kernel = kb.Build();
        modelInfo ??= config.Model;
        _model = _kernel.GetRequiredService<ITextEmbeddingGenerationService>(modelInfo.Name);
        _modelInfo = modelInfo;
    }

    /// <summary>
    /// Create a new text embedding model that uses the given embedding generation service
    /// </summary>
    /// <param name="service"></param>
    /// <param name="modelInfo">information about the model to create</param>
    public TextEmbeddingModel(ITextEmbeddingGenerationService service, ModelInfo? modelInfo)
    {
        ArgumentVerify.ThrowIfNull(service, nameof(service));
        _model = service;
        _modelInfo = modelInfo;
    }

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
