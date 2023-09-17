// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Embeddings;

public class TextEmbeddingModel
{
    ITextEmbeddingGeneration _model;
    ModelInfo _modelInfo;

    /// <summary>
    /// Create a new text embedding model from the Open AI config
    /// By default, uses model in config.Model
    /// </summary>
    /// <param name="config">Open AI configuration</param>
    /// <param name="modelInfo">information about the model to create</param>
    public TextEmbeddingModel(OpenAIConfig config, ModelInfo? modelInfo = null)
    {
        ArgumentVerify.ThrowIfNull(config, nameof(config));
        config.Validate();

        modelInfo ??= config.Model;
        IKernel kernel = config.CreateKernel();
        _model = kernel.GetService<ITextEmbeddingGeneration>(modelInfo.Name);
        _modelInfo = modelInfo;
    }

    /// <summary>
    /// Create a new text embedding model that uses the given embedding generation service
    /// </summary>
    /// <param name="service"></param>
    /// <param name="modelInfo">information about the model to create</param>
    public TextEmbeddingModel(ITextEmbeddingGeneration service, ModelInfo? modelInfo)
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
        return _model.GenerateEmbeddingAsync(text, cancelToken);
    }
}
