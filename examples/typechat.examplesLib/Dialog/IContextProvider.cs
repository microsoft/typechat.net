// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Dialog;

/// <summary>
/// Applications that work the language models typically need dynamic retrieval of context pertinent to a
/// particular request. This is the so called RAG pattern. 
/// </summary>
public interface IContextProvider
{
    /// <summary>
    /// Return relevant context for this this request
    /// </summary>
    /// <param name="request">user request</param>
    /// <param name="cancellationToken">optional cancel token</param>
    /// <returns></returns>
    IAsyncEnumerable<IPromptSection> GetContextAsync(string request, CancellationToken cancellationToken = default);
}
