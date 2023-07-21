// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.SemanticKernel;

public class OpenAIConfig
{
    public static class VariableNames
    {
        public const string OPENAI_API_KEY = "OPENAI_API_KEY";
        public const string AZURE_API_KEY = "AZURE_API_KEY";
        public const string OPENAI_ENDPOINT = "OPENAI_ENDPOINT";
        public const string OPENAI_ORGANIZATION = "OPENAI_ORGANIZATION";
    }

    public OpenAIConfig() { }

    public bool Azure { get; set; } = true;
    public string Endpoint { get; set; }
    public string ApiKey { get; set; }
    public string? Organization { get; set; }

    public void Validate()
    {
        ArgumentException.ThrowIfNullOrEmpty(Endpoint, nameof(Endpoint));
        ArgumentException.ThrowIfNullOrEmpty(ApiKey, nameof(ApiKey));
    }

    public static OpenAIConfig FromEnvironment(bool isAzure)
    {
        OpenAIConfig config = new OpenAIConfig();
        config.Endpoint = Environment.GetEnvironmentVariable(VariableNames.OPENAI_ENDPOINT);
        if (isAzure)
        {
            config.ApiKey = Environment.GetEnvironmentVariable(VariableNames.AZURE_API_KEY);
        }
        else
        {
            config.ApiKey = Environment.GetEnvironmentVariable(VariableNames.OPENAI_API_KEY);
            config.Organization = Environment.GetEnvironmentVariable(VariableNames.OPENAI_ORGANIZATION);
        }
        return config;
    }
}
