// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

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

    [JsonPropertyName("name")]
    public string Name => _name;

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
