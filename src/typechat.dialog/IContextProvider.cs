// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Dialog;

/// <summary>
/// Applications that work the language models typically need dynamic retrieval of context pertinent to a
/// particular reques. This is the so called RAG pattern. 
/// </summary>
public interface IContextProvider
{
    //IEnumerable<IPromptSection>? GetContext(string request);
    IAsyncEnumerable<IPromptSection> GetContextAsync(string request, CancellationToken cancelToken);
}
