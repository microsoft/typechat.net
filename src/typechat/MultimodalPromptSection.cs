// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// A prompt section that can contain a mix of text and images.
/// <para>
/// Build a section by adding text and image parts in the order they should appear:
/// <code>
/// var section = new MultimodalPromptSection()
///     .AddText("What is in this image?")
///     .AddImage(PromptImage.FromFile("cat.png"));
/// </code>
/// </para>
/// Language models that support images (such as GPT-4-vision, GPT-4-omni and GPT-4-turbo) will receive
/// the images. Models that only support text will receive the concatenated text via <see cref="GetText"/>.
/// </summary>
public class MultimodalPromptSection : IMultimodalPromptSection
{
    private readonly string _source;
    private readonly List<PromptContentPart> _parts;

    /// <summary>
    /// Create a new, empty multimodal prompt section.
    /// </summary>
    /// <param name="source">The source of the section. Defaults to <see cref="PromptSection.Sources.User"/></param>
    public MultimodalPromptSection(string? source = null)
    {
        _source = string.IsNullOrEmpty(source) ? PromptSection.Sources.User : source!;
        _parts = new List<PromptContentPart>();
    }

    /// <summary>
    /// Create a new multimodal prompt section from a set of content parts.
    /// </summary>
    /// <param name="source">The source of the section</param>
    /// <param name="parts">The content parts</param>
    public MultimodalPromptSection(string source, IEnumerable<PromptContentPart> parts)
        : this(source)
    {
        ArgumentVerify.ThrowIfNull(parts, nameof(parts));
        foreach (var part in parts)
        {
            Add(part);
        }
    }

    /// <summary>
    /// The source of this section. Typical sources are defined in <see cref="PromptSection.Sources"/>.
    /// </summary>
    public string? Source => _source;

    /// <summary>
    /// The ordered content parts (text and images) that make up this section.
    /// </summary>
    public IReadOnlyList<PromptContentPart> ContentParts => _parts;

    /// <summary>
    /// True when this section contains at least one image part.
    /// </summary>
    public bool HasImages
    {
        get
        {
            for (int i = 0; i < _parts.Count; ++i)
            {
                if (_parts[i].IsImage)
                {
                    return true;
                }
            }
            return false;
        }
    }

    /// <summary>
    /// Add a content part to the section.
    /// </summary>
    /// <param name="part">the part to add</param>
    /// <returns>this section, to allow chaining</returns>
    public MultimodalPromptSection Add(PromptContentPart part)
    {
        ArgumentVerify.ThrowIfNull(part, nameof(part));
        _parts.Add(part);
        return this;
    }

    /// <summary>
    /// Add a text part to the section.
    /// </summary>
    /// <param name="text">the text to add</param>
    /// <returns>this section, to allow chaining</returns>
    public MultimodalPromptSection AddText(string text) => Add(PromptContentPart.FromText(text));

    /// <summary>
    /// Add an image part to the section.
    /// </summary>
    /// <param name="image">the image to add</param>
    /// <returns>this section, to allow chaining</returns>
    public MultimodalPromptSection AddImage(PromptImage image) => Add(PromptContentPart.FromImage(image));

    /// <summary>
    /// Add an image part to the section.
    /// </summary>
    /// <param name="url">An http(s) url to a hosted image, or a data uri containing base64 encoded image bytes</param>
    /// <param name="detail">Controls how the model should process the image</param>
    /// <returns>this section, to allow chaining</returns>
    public MultimodalPromptSection AddImage(string url, ImageDetail detail = ImageDetail.Auto) => AddImage(new PromptImage(url, detail));

    /// <summary>
    /// Return the concatenation of all text parts in this section. Image parts are ignored.
    /// </summary>
    /// <returns>string</returns>
    public string GetText()
    {
        if (_parts.Count == 0)
        {
            return string.Empty;
        }
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < _parts.Count; ++i)
        {
            PromptContentPart part = _parts[i];
            if (part.IsText && !string.IsNullOrEmpty(part.Text))
            {
                if (sb.Length > 0)
                {
                    sb.Append('\n');
                }
                sb.Append(part.Text);
            }
        }
        return sb.ToString();
    }

    /// <summary>
    /// Create a new multimodal section whose source is <see cref="PromptSection.Sources.User"/>.
    /// </summary>
    /// <returns>MultimodalPromptSection</returns>
    public static MultimodalPromptSection FromUser() => new MultimodalPromptSection(PromptSection.Sources.User);
}
