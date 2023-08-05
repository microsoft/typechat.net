// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel.Reliability;

namespace Microsoft.TypeChat.SemanticKernel;

public class OpenAIConfig
{
    public static class VariableNames
    {
        public const string OPENAI_API_KEY = "OPENAI_API_KEY";
        public const string OPENAI_ENDPOINT = "OPENAI_ENDPOINT";
        public const string OPENAI_ORGANIZATION = "OPENAI_ORGANIZATION";
        public const string OPENAI_MODEL = "OPENAI_MODEL";
        public const string AZURE_OPENAI_API_KEY = "AZURE_OPENAI_API_KEY";
        public const string AZURE_OPENAI_ENDPOINT = "AZURE_OPENAI_ENDPOINT";
    }

    public OpenAIConfig() { }

    public bool Azure { get; set; } = true;
    public string Endpoint { get; set; }
    public string ApiKey { get; set; }
    public string? Organization { get; set; }
    public string? Model { get; set; }
    /// <summary>
    /// Http Settings
    /// </summary>
    public int TimeoutMs { get; set; } = 15 * 1000;
    public int MaxRetries { get; set; } = 3;
    public int MaxPauseMs { get; set; } = 100;

    public void Validate()
    {
        ArgumentException.ThrowIfNullOrEmpty(Endpoint, nameof(Endpoint));
        ArgumentException.ThrowIfNullOrEmpty(ApiKey, nameof(ApiKey));
    }

    public static OpenAIConfig FromEnvironment()
    {
        OpenAIConfig config = new OpenAIConfig();
        config.ApiKey = Environment.GetEnvironmentVariable(VariableNames.AZURE_OPENAI_API_KEY);
        if (config.ApiKey == null)
        {
            config.Azure = false;
            config.ApiKey = Environment.GetEnvironmentVariable(VariableNames.OPENAI_API_KEY);
            config.Endpoint = Environment.GetEnvironmentVariable(VariableNames.OPENAI_ENDPOINT);
            config.Organization = Environment.GetEnvironmentVariable(VariableNames.OPENAI_ORGANIZATION);
        }
        else
        {
            config.Endpoint = Environment.GetEnvironmentVariable(VariableNames.AZURE_OPENAI_ENDPOINT);
        }
        config.Model = Environment.GetEnvironmentVariable(VariableNames.OPENAI_MODEL);
        config.Validate();
        return config;
    }
}
