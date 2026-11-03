// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// Optional capability for models that return completion metadata in addition to text.
/// Existing models only need to implement <see cref="ILanguageModel"/>.
/// </summary>
public interface ILanguageModelWithMetadata : ILanguageModel
{
    /// <summary>
    /// Get completion text and any metadata reported by the model.
    /// </summary>
    /// <param name="prompt">prompt</param>
    /// <param name="settings">translation settings such as temperature</param>
    /// <param name="cancelToken">cancellation token</param>
    /// <returns>The completion text and optional metadata.</returns>
    Task<LanguageModelResponse> CompleteWithMetadataAsync(Prompt prompt, TranslationSettings? settings = null, CancellationToken cancelToken = default);
}
