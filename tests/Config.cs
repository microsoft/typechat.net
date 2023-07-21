// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Tests;

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
}
