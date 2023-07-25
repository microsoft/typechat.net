// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public class Config
{
    public static class ModelNames
    {
        public const string Gpt35Turbo = "gpt-35-turbo";
        public const string Gpt4 = "gpt-4";
    }

    public static T LoadConfig<T>(string configFile, string configOverloadFile, string sectionName = default)
        where T : new()
    {
        var config = new ConfigurationBuilder()
                    .AddJsonFile(configFile, false, true)
                    .AddJsonFile(configOverloadFile, false, true)
                    .Build();
        var configSection = config.GetSection(sectionName);
        if (configSection == null)
        {
            throw new ArgumentException($"{sectionName} not found");
        }

        T settings = new T();
        configSection.Bind(settings);
        return settings;
    }

    public static OpenAIConfig LoadOpenAI()
    {
        return LoadConfig<OpenAIConfig>("appSettings.json", "appSettings.Development.json", "OpenAI");
    }

    OpenAIConfig? _openAI;

    public Config()
    {
        _openAI = LoadOpenAI();
    }

    public OpenAIConfig OpenAI => _openAI;

    public bool HasOpenAI => (_openAI != null);

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
            if (line.StartsWith('#'))
            {
                continue;
            }

            string[] envVars = line.Split('=', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
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
