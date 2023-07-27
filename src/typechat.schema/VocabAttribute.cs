// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class VocabAttribute : Attribute
{
    public VocabAttribute() { }

    public VocabAttribute(string vocabName)
    {
        ArgumentException.ThrowIfNullOrEmpty(vocabName, nameof(vocabName));
        Name = vocabName;
    }

    public string? Name { get; set; }
    public bool Inline { get; set; } = true;

    public bool HasName => !string.IsNullOrEmpty(Name);
}
