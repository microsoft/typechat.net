// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public interface IPromptSection
{
    /// <summary>
    /// The source of this prompt section
    /// </summary>
    public string? Source { get; }
    /// <summary>
    /// The text for this section
    /// </summary>
    /// <returns></returns>
    string GetText();
}
