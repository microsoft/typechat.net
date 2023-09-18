// Copyright (c) Microsoft. All rights reserved.

using Microsoft.TypeChat.Classification;

namespace Microsoft.TypeChat;

/// <summary>
/// Some scenarios have schemas that can be partioned into sub-schemas or translators.
/// Language models have token limits, which means we must do a first pass and route requests to the appropriate target
/// It may also be necessary to bind to these translators dynamically
/// 
/// A DynamicJsonTranslator is a simple aggregate translator that routes a request to child JsonTranslators. 
/// </summary>
public class HierarchicalJsonTranslator : IJsonTranslator
{
    ITextRequestRouter<IJsonTranslator> _requestRouter;

    /// <summary>
    /// Create a new JsonTranslator that routes requests to child translators
    /// </summary>
    /// <param name="router"></param>
    public HierarchicalJsonTranslator(ITextRequestRouter<IJsonTranslator> router)
    {
        ArgumentVerify.ThrowIfNull(router, nameof(router));
        _requestRouter = router;
    }

    /// <summary>
    /// The router being used by this translator
    /// </summary>
    public ITextRequestRouter<IJsonTranslator> Router => _requestRouter;

    public async Task<object> TranslateToObjectAsync(string request, CancellationToken cancelToken)
    {
        // First, select the translator that is best suited to translate this request
        IJsonTranslator? translator = await _requestRouter.RouteRequestAsync(request, cancelToken);
        if (translator == null)
        {
            throw new TypeChatException(TypeChatException.ErrorCode.NoTranslator, request);
        }
        return await translator.TranslateToObjectAsync(request, cancelToken);
    }
}
