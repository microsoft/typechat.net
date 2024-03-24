// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Classification;

/// <summary>
/// A Text Classification Response from the Translator
/// </summary>
public class TextClassification
{
    /// <summary>
    /// The class assigned to the request
    /// </summary>
    [JsonPropertyName("class")]
    [JsonVocab(Name = "Classes", PropertyName = "class")]
    [Comment("Use this for the classification")]
    public string Class { get; set; }

    [JsonIgnore]
    public bool HasClass => !string.IsNullOrEmpty(Class);

    public override string ToString() => Class;
}

