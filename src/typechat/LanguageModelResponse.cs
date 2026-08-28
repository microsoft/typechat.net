// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// The result of an <see cref="ILanguageModel.CompleteAsync"/> call: the completion text plus
/// optional <see cref="CompletionInfo"/> metadata (token usage, finish reason, and so on).
///
/// A <see cref="LanguageModelResponse"/> implicitly converts to and from <see cref="string"/>, so
/// existing code that treats a completion as a bare string continues to work.
/// </summary>
public class LanguageModelResponse
{
    /// <summary>
    /// Creates a new <see cref="LanguageModelResponse"/>.
    /// </summary>
    /// <param name="text">completion text</param>
    /// <param name="info">optional completion metadata</param>
    public LanguageModelResponse(string text, CompletionInfo? info = null)
    {
        Text = text ?? string.Empty;
        Info = info;
    }

    /// <summary>
    /// The completion text returned by the model.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Metadata reported alongside the completion, when available.
    /// </summary>
    public CompletionInfo? Info { get; set; }

    /// <summary>
    /// Implicitly returns the completion <see cref="Text"/>, so a response can be used wherever a
    /// string is expected.
    /// </summary>
    public static implicit operator string(LanguageModelResponse response)
    {
        return response?.Text ?? string.Empty;
    }

    /// <summary>
    /// Implicitly wraps a string as a <see cref="LanguageModelResponse"/> with no metadata.
    /// </summary>
    public static implicit operator LanguageModelResponse(string text)
    {
        return new LanguageModelResponse(text);
    }

    /// <inheritdoc/>
    public override string ToString() => Text;
}
