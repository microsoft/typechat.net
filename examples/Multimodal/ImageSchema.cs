// Copyright (c) Microsoft. All rights reserved.

using System.Text.Json.Serialization;
using Microsoft.TypeChat.Schema;

namespace Multimodal;

/// <summary>
/// A strongly typed description of an image, produced by translating a multimodal prompt
/// (text + image) into this type.
/// </summary>
public class ImageResponse
{
    [Comment("A single sentence caption that describes the image")]
    [JsonPropertyName("caption")]
    public string Caption { get; set; }

    [Comment("The most prominent objects visible in the image")]
    [JsonPropertyName("objects")]
    public string[] Objects { get; set; }

    [Comment("The dominant colors in the image")]
    [JsonPropertyName("colors")]
    public string[] Colors { get; set; }

    [Comment("True if the image contains any text")]
    [JsonPropertyName("containsText")]
    public bool ContainsText { get; set; }
}
