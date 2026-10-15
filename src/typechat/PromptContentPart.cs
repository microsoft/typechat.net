// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// The kind of content carried by a <see cref="PromptContentPart"/>.
/// </summary>
public enum PromptContentType
{
    /// <summary>
    /// The part contains text.
    /// </summary>
    Text,

    /// <summary>
    /// The part contains an image.
    /// </summary>
    Image,
}

/// <summary>
/// A single part of a multimodal prompt section. A part is either text or an image.
/// A <see cref="MultimodalPromptSection"/> is an ordered collection of these parts, allowing
/// text and images to be interleaved within one prompt section.
/// </summary>
public sealed class PromptContentPart
{
    private readonly string? _text;
    private readonly PromptImage? _image;

    private PromptContentPart(PromptContentType type, string? text, PromptImage? image)
    {
        Type = type;
        _text = text;
        _image = image;
    }

    /// <summary>
    /// The kind of content this part carries.
    /// </summary>
    public PromptContentType Type { get; }

    /// <summary>
    /// True when this part contains text.
    /// </summary>
    public bool IsText => Type == PromptContentType.Text;

    /// <summary>
    /// True when this part contains an image.
    /// </summary>
    public bool IsImage => Type == PromptContentType.Image;

    /// <summary>
    /// The text of this part, or null when this is not a text part.
    /// </summary>
    public string? Text => _text;

    /// <summary>
    /// The image of this part, or null when this is not an image part.
    /// </summary>
    public PromptImage? Image => _image;

    /// <summary>
    /// Create a text content part.
    /// </summary>
    /// <param name="text">text</param>
    /// <returns>PromptContentPart</returns>
    public static PromptContentPart FromText(string text)
    {
        ArgumentVerify.ThrowIfNull(text, nameof(text));
        return new PromptContentPart(PromptContentType.Text, text, null);
    }

    /// <summary>
    /// Create an image content part.
    /// </summary>
    /// <param name="image">image</param>
    /// <returns>PromptContentPart</returns>
    public static PromptContentPart FromImage(PromptImage image)
    {
        ArgumentVerify.ThrowIfNull(image, nameof(image));
        return new PromptContentPart(PromptContentType.Image, null, image);
    }

    /// <summary>
    /// Implicitly create a text content part from a string.
    /// </summary>
    /// <param name="text">text</param>
    public static implicit operator PromptContentPart(string text) => FromText(text);

    /// <summary>
    /// Implicitly create an image content part from a <see cref="PromptImage"/>.
    /// </summary>
    /// <param name="image">image</param>
    public static implicit operator PromptContentPart(PromptImage image) => FromImage(image);
}
