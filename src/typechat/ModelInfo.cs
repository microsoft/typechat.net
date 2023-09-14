// Copyright (c) Microsoft. All rights reserved.

using System.Reflection;

namespace Microsoft.TypeChat;

/// <summary>
/// Language Model Information
/// </summary>
public class ModelInfo
{
    string _name;
    int _maxTokens;
    int _maxCharCount;
    double _tokenToCharMultiple;

    [JsonConstructor]
    public ModelInfo(string name, int maxTokens, double tokenToCharMultiple = 2.5)
    {
        ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));

        _name = name;
        _maxTokens = maxTokens;
        _tokenToCharMultiple = tokenToCharMultiple;
        _maxCharCount = (int)((double)maxTokens * tokenToCharMultiple);
    }

    /// <summary>
    /// Modelname
    /// </summary>
    [JsonPropertyName("name")]
    public string Name => _name;
    /// <summary>
    /// Maximum tokens allowed by this model
    /// </summary>
    [JsonPropertyName("maxTokens")]
    public int MaxTokens => _maxTokens;
    /// <summary>
    /// Allows a simple way to estimate # of tokens from # of characters
    /// </summary>
    [JsonPropertyName("tokenMultiple")]
    public double TokenToCharMultiple => _tokenToCharMultiple;
    /// <summary>
    /// An estimate of the max # of characters - input + output - that the model will accept
    /// </summary>
    [JsonIgnore]
    public int MaxCharCount => _maxCharCount;

    public static implicit operator ModelInfo(string name)
    {
        return new ModelInfo(name, 4096);
    }
}
