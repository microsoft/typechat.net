// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// A prompt section whose content is multimodal: an ordered list of <see cref="PromptContentPart"/>,
/// each of which is either text or an image.
/// <para>
/// This extends <see cref="IPromptSection"/> so that language models which do not support images can
/// continue to consume the section as text via <see cref="IPromptSection.GetText"/>, while models that
/// support images can read the individual <see cref="ContentParts"/>.
/// </para>
/// </summary>
public interface IMultimodalPromptSection : IPromptSection
{
    /// <summary>
    /// The ordered content parts (text and images) that make up this section.
    /// </summary>
    IReadOnlyList<PromptContentPart> ContentParts { get; }
}
