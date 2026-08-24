// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// An image referenced by a multimodal prompt section.
/// The image is identified by a <see cref="Url"/>, which may be either:
/// <list type="bullet">
/// <item>An http(s) url to a hosted image</item>
/// <item>A data uri that embeds base64 encoded image bytes: <c>data:{mediaType};base64,{data}</c></item>
/// </list>
/// Models such as GPT-4-vision, GPT-4-omni and GPT-4-turbo can accept images as part of a prompt.
/// </summary>
public class PromptImage
{
    /// <summary>
    /// Create a new PromptImage
    /// </summary>
    /// <param name="url">An http(s) url to a hosted image, or a data uri containing base64 encoded image bytes</param>
    /// <param name="detail">Controls how the model should process the image</param>
    public PromptImage(string url, ImageDetail detail = ImageDetail.Auto)
    {
        ArgumentVerify.ThrowIfNullOrEmpty(url, nameof(url));
        Url = url;
        Detail = detail;
    }

    /// <summary>
    /// An http(s) url to a hosted image, or a data uri containing base64 encoded image bytes.
    /// </summary>
    public string Url { get; }

    /// <summary>
    /// Controls how the model should process the image.
    /// </summary>
    public ImageDetail Detail { get; }

    /// <summary>
    /// True when <see cref="Url"/> is a data uri that embeds the image bytes, rather than a remote url.
    /// </summary>
    public bool IsDataUri => Url.StartsWith("data:", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Create a PromptImage from raw image bytes. The bytes are encoded as a base64 data uri.
    /// </summary>
    /// <param name="bytes">image bytes</param>
    /// <param name="mediaType">the image media (MIME) type, e.g. "image/png"</param>
    /// <param name="detail">Controls how the model should process the image</param>
    /// <returns>PromptImage</returns>
    public static PromptImage FromBytes(byte[] bytes, string mediaType = "image/png", ImageDetail detail = ImageDetail.Auto)
    {
        ArgumentVerify.ThrowIfNull(bytes, nameof(bytes));
        ArgumentVerify.ThrowIfNullOrEmpty(mediaType, nameof(mediaType));
        string dataUri = $"data:{mediaType};base64,{Convert.ToBase64String(bytes)}";
        return new PromptImage(dataUri, detail);
    }

    /// <summary>
    /// Create a PromptImage by loading an image file and encoding its bytes as a base64 data uri.
    /// The media type is inferred from the file extension unless <paramref name="mediaType"/> is supplied.
    /// </summary>
    /// <param name="filePath">path to the image file</param>
    /// <param name="mediaType">(optional) the image media (MIME) type. Inferred from the file extension when null</param>
    /// <param name="detail">Controls how the model should process the image</param>
    /// <returns>PromptImage</returns>
    public static PromptImage FromFile(string filePath, string? mediaType = null, ImageDetail detail = ImageDetail.Auto)
    {
        ArgumentVerify.ThrowIfNullOrEmpty(filePath, nameof(filePath));
        mediaType ??= GetMediaType(filePath);
        byte[] bytes = File.ReadAllBytes(filePath);
        return FromBytes(bytes, mediaType, detail);
    }

    /// <summary>
    /// Infer an image media (MIME) type from a file name or url extension.
    /// Returns the wildcard type "image/*" when the extension is not recognized.
    /// </summary>
    /// <param name="pathOrUrl">a file path or url</param>
    /// <returns>media type</returns>
    public static string GetMediaType(string pathOrUrl)
    {
        if (string.IsNullOrEmpty(pathOrUrl))
        {
            return "image/*";
        }
        string ext = Path.GetExtension(pathOrUrl).ToLowerInvariant();
        switch (ext)
        {
            case ".png":
                return "image/png";
            case ".jpg":
            case ".jpeg":
                return "image/jpeg";
            case ".gif":
                return "image/gif";
            case ".webp":
                return "image/webp";
            case ".bmp":
                return "image/bmp";
            case ".tif":
            case ".tiff":
                return "image/tiff";
            case ".svg":
                return "image/svg+xml";
            default:
                return "image/*";
        }
    }
}
