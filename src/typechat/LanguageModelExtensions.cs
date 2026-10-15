// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// Optional completion capabilities with support for existing language models.
/// </summary>
public static class LanguageModelExtensions
{
    /// <summary>
    /// Get completion metadata when supported. For a text-only model, returns its completion
    /// with null metadata without changing the model's exception or cancellation behavior.
    /// </summary>
    /// <param name="model">language model</param>
    /// <param name="prompt">prompt</param>
    /// <param name="settings">translation settings such as temperature</param>
    /// <param name="cancelToken">cancellation token</param>
    /// <returns>The completion text and optional metadata.</returns>
    public static async Task<LanguageModelResponse> CompleteWithMetadataAsync(
        this ILanguageModel model,
        Prompt prompt,
        TranslationSettings? settings = null,
        CancellationToken cancelToken = default)
    {
        ArgumentVerify.ThrowIfNull(model, nameof(model));
        if (model is ILanguageModelWithMetadata metadataModel)
        {
            return await metadataModel.CompleteWithMetadataAsync(prompt, settings, cancelToken).ConfigureAwait(false);
        }
        string text = await model.CompleteAsync(prompt, settings, cancelToken).ConfigureAwait(false);
        return new LanguageModelResponse(text);
    }
}
