// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Extensions.AI;
using Microsoft.TypeChat.Embeddings;

namespace Microsoft.TypeChat;

public class TextEmbeddingModel
{
    private readonly Kernel _kernel;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _model;
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
        _model = _kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>(modelInfo.Name);
        _modelInfo = modelInfo;
    }

    /// <summary>
    /// Create a new text embedding model that uses the given embedding generation service
    /// </summary>
    /// <param name="service"></param>
    /// <param name="modelInfo">information about the model to create</param>
    public TextEmbeddingModel(IEmbeddingGenerator<string, Embedding<float>> service, ModelInfo? modelInfo)
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
    public Task<Embedding<float>> GenerateEmbeddingAsync(string text, CancellationToken cancelToken = default)
    {
        return _model.GenerateEmbeddingAsync(text, _kernel, cancelToken);
    }

    /// <summary>
    /// Generate embeddings in a batch
    /// </summary>
    /// <param name="texts">list of texts to turn into embeddings</param>
    /// <param name="cancelToken">optional cancel token</param>
    /// <returns></returns>
    public async Task<Embedding<float>[]> GenerateEmbeddingsAsync(IList<string> texts, CancellationToken cancelToken = default)
    {
        var results = await _model.GenerateAsync(texts, null, cancelToken);
        Embedding<float>[] embeddings = new Embedding<float>[results.Count];
        for (int i = 0; i < embeddings.Length; ++i)
        {
            embeddings[i] = results[i];
        }
        return embeddings;
    }
}
