// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public class Config
{
    public const string DefaultConfigFile = "appSettings.json";
    public const string DefaultConfigFile_Dev = "appSettings.Development.json";

    public static class ModelNames
    {
        public const string Gpt35Turbo = "gpt-35-turbo";
        public const string Gpt4 = "gpt-4";
        public const string Ad002 = "ada-002";
    }

    public static T LoadConfig<T>(string configFile, string configOverloadFile, string sectionName = default)
        where T : new()
    {
        var configBuilder = new ConfigurationBuilder()
                    .AddJsonFile(configFile, false, true);

        if (File.Exists(configOverloadFile))
        {
            configBuilder.AddJsonFile(configOverloadFile, false, true);
        }
        var config = configBuilder.Build();
        var configSection = config.GetSection(sectionName);
        if (configSection == null)
        {
            throw new ArgumentException($"{sectionName} not found");
        }

        T settings = new T();
        configSection.Bind(settings);
        return settings;
    }

    public static OpenAIConfig LoadOpenAI(string? sectionName = null)
    {
        sectionName ??= "OpenAI";
        OpenAIConfig config = LoadConfig<OpenAIConfig>(DefaultConfigFile, DefaultConfigFile_Dev, sectionName);
        return config;
    }

    OpenAIConfig? _openAI;
    OpenAIConfig? _openAIEmbeddings;

    public Config()
    {
        _openAI = LoadOpenAI();
        _openAIEmbeddings = LoadOpenAI("OpenAI_Embeddings");
    }

    public OpenAIConfig OpenAI => _openAI;
    public OpenAIConfig? OpenAIEmbeddings => _openAIEmbeddings;

    public bool HasOpenAI => (_openAI != null);
    public bool HasOpenAIEmbeddings => (_openAIEmbeddings != null);

    /// <summary>
    /// Load a file in .env format. Apply contained variables as process specific environment
    /// variables
    /// </summary>
    /// <param name="filePath">Path to env file</param>
    public static void ApplyEnvFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return;
        }

        using StreamReader reader = new StreamReader(filePath);
        string line = null;
        while ((line = reader.ReadLine()) != null)
        {
            if (line.StartsWith("#"))
            {
                continue;
            }

            string[] envVars = line.Split('=', (char)StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray(); ;
            if (envVars != null)
            {
                UpdateVariable(envVars);
            }
        }
    }

    static void UpdateVariable(string[] nvPairs)
    {
        if (nvPairs != null && nvPairs.Length > 0)
        {
            Environment.SetEnvironmentVariable(
                nvPairs[0], // name
                (nvPairs.Length > 1) ? nvPairs[1] : null, // value
                EnvironmentVariableTarget.Process
            );
        }
    }

}
