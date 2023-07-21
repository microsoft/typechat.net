// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public class ModelInfo
{
    string _name;
    int _maxTokens;
    int _outputSize;

    [JsonConstructor]
    public ModelInfo(string name, int maxTokens, int outputSize = 1536)
    {
        ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));
        _name = name;
        _maxTokens = maxTokens;
        _outputSize = outputSize;
    }

    [JsonPropertyName("name")]
    public string Name => _name;

    [JsonPropertyName("maxTokens")]
    public int MaxTokens => _maxTokens;

    [JsonPropertyName("outputSize")]
    public int OutputSize => _outputSize;

    public static implicit operator ModelInfo(string name)
    {
        return new ModelInfo(name, 4096);
    }
}
