// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// Some scenarios have schemas that can be partitioned into sub-schemas or translators.
/// Language models have token limits, which means we must do a first pass and route requests to the appropriate target
/// It may also be necessary to bind to these translators dynamically
/// 
/// HierarchicalJsonTranslator demonstrates how a translator can use an in-memory vector index to semantically route request to child translators 
/// </summary>
public class HierarchicalJsonTranslator : IJsonTranslator
{
    private ILanguageModel _model;

    /// <summary>
    /// Create a new JsonTranslator that routes requests to child translators
    /// </summary>
    /// <param name="model">language model to use for translators</param>
    /// <param name="embeddingModel">embedding model to use for translators</param>
    public HierarchicalJsonTranslator(ILanguageModel model, TextEmbeddingModel embeddingModel)
    {
        ArgumentVerify.ThrowIfNull(model, nameof(model));
        ArgumentVerify.ThrowIfNull(embeddingModel, nameof(embeddingModel));
        _model = model;
        Router = new VectorTextIndex<IJsonTranslator>(embeddingModel);
    }

    /// <summary>
    /// The router being used by this translator
    /// </summary>
    public VectorTextIndex<IJsonTranslator> Router { get; }

    /// <summary>
    /// Add a JsonTranslator with this description
    /// </summary>
    /// <typeparam name="T">type of translator</typeparam>
    /// <param name="description">description of the translator </param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns></returns>
    public virtual Task AddSchemaAsync<T>(string description, CancellationToken cancellationToken = default)
        => Router.AddAsync(new JsonTranslator<T>(_model), description, cancellationToken);

    public async Task<object> TranslateToObjectAsync(string request, CancellationToken cancellationToken = default)
    {
        // First, select the translator that is best suited to translate this request
        IJsonTranslator? translator = await Router.RouteRequestAsync(request, cancellationToken).ConfigureAwait(false);
        if (translator is null)
        {
            throw new TypeChatException(TypeChatException.ErrorCode.NoTranslator, request);
        }

        return await translator.TranslateToObjectAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
