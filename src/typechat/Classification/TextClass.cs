// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Classification;

/// <summary>
/// A Text Class has a name and a description.
/// The model returns the name of the class whose description most closely matches a request
/// </summary>
public struct TextClass
{
    /// <summary>
    /// Create
    /// </summary>
    /// <param name="name"></param>
    /// <param name="description"></param>
    public TextClass(string name, string description)
    {
        ArgumentVerify.ThrowIfNullOrEmpty(name, nameof(name));
        ArgumentVerify.ThrowIfNullOrEmpty(description, nameof(description));
        Name = name;
        Description = description;
    }

    /// <summary>
    /// Class name
    /// </summary>
    public string Name { get; private set; }
    /// <summary>
    /// Class description
    /// The description of a class has strong influence on how the model classifies a user's input
    /// </summary>
    public string Description { get; private set; }

    public override string ToString()
    {
        return $"{Name}: {Description}";
    }
}

