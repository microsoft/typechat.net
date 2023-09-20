// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel.Reliability;

namespace Microsoft.TypeChat;

/// <summary>
/// Open AI configuration
/// Can be initialized either from environment variables or from config files
/// </summary>
public class OpenAIConfig
{
    /// <summary>
    /// Names of environment variables
    /// </summary>
    public static class VariableNames
    {
        /// <summary>
        /// Api key to use with the Open AI service
        /// </summary>
        public const string OPENAI_API_KEY = "OPENAI_API_KEY";
        /// <summary>
        /// The Open AI endpoint. This follows semantic kernel conventions
        /// https://api.openai.com/v1/chat/completions
        /// </summary>
        public const string OPENAI_ENDPOINT = "OPENAI_ENDPOINT";
        /// <summary>
        /// Optional: OpenAI organization
        /// </summary>
        public const string OPENAI_ORGANIZATION = "OPENAI_ORGANIZATION";
        /// <summary>
        /// Name of the language model to use
        /// </summary>
        public const string OPENAI_MODEL = "OPENAI_MODEL";
        /// <summary>
        /// Name of the embedding model to use
        /// </summary>
        public const string OPENAI_EMBEDDINGMODEL = "OPENAI_EMBEDDINGMODEL";
        /// <summary>
        /// Api key to use for Azure Open AI service
        /// </summary>
        public const string AZURE_OPENAI_API_KEY = "AZURE_OPENAI_API_KEY";
        /// <summary>
        /// Endpoint to use for Azure OpenAI service.
        /// This follows the Semantic Kernel convention
        /// https://YOUR_RESOURCE_NAME.openai.azure.com
        /// </summary>
        public const string AZURE_OPENAI_ENDPOINT = "AZURE_OPENAI_ENDPOINT";
    }

    public OpenAIConfig() { }

    /// <summary>
    /// Use Azure Open AI?
    /// </summary>
    public bool Azure { get; set; } = true;
    /// <summary>
    /// Api endpoint
    /// </summary>
    public string Endpoint { get; set; }
    /// <summary>
    /// Api key to use
    /// </summary>
    public string ApiKey { get; set; }
    /// <summary>
    /// Organization: only used by Open AI service. 
    /// </summary>
    public string? Organization { get; set; }
    /// <summary>
    /// Model name
    /// </summary>
    public string? Model { get; set; }
    /// <summary>
    /// Http Settings
    /// </summary>
    public int TimeoutMs { get; set; } = 15 * 1000;
    public int MaxRetries { get; set; } = 3;
    public int MaxPauseMs { get; set; } = 100;

    /// <summary>
    /// Validate the configuration
    /// </summary>
    /// <param name="configFileName"></param>
    internal void Validate(string configFileName = default)
    {
        configFileName ??= string.Empty;

        Verify(Endpoint, nameof(Endpoint), configFileName);
        Verify(ApiKey, nameof(ApiKey), configFileName);
    }

    void Verify(string value, string name, string fileName)
    {
        if (string.IsNullOrEmpty(value) || value == "?")
        {
            throw new ArgumentException($"OpenAIConfig: {name} is not initialized in {fileName}");
        }
    }

    /// <summary>
    /// Load configuration from environment variables
    /// </summary>
    /// <param name="isEmbedding">Is this an embedding model?</param>
    /// <returns></returns>
    public static OpenAIConfig FromEnvironment(bool isEmbedding = false)
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
        if (isEmbedding)
        {
            config.Model = Environment.GetEnvironmentVariable(VariableNames.OPENAI_EMBEDDINGMODEL);
        }
        else
        {
            config.Model = Environment.GetEnvironmentVariable(VariableNames.OPENAI_MODEL);
        }
        config.Validate();
        return config;
    }
}
