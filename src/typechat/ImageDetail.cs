// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// Controls how a model processes an image supplied in a multimodal prompt and generates
/// its textual understanding of the image.
/// Mirrors the OpenAI image "detail" setting.
/// </summary>
public enum ImageDetail
{
    /// <summary>
    /// Let the model choose how to process the image.
    /// </summary>
    Auto,

    /// <summary>
    /// The model treats the image as a low resolution 512x512px image.
    /// </summary>
    Low,

    /// <summary>
    /// The model considers the image at full size.
    /// </summary>
    High,
}
