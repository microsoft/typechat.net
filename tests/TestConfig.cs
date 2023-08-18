// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Tests;

public class TestConfig : TypeChatTest
{
    [Fact]
    public void TestEnvOpenAI()
    {
        string? prevEndpoint = null;
        string? prevKey = null;
        string? prevOrg = null;
        try
        {
            prevEndpoint = SetEnv(OpenAIConfig.VariableNames.OPENAI_ENDPOINT, "O_ENDPOINT");
            prevKey = SetEnv(OpenAIConfig.VariableNames.OPENAI_API_KEY, "O_API");
            prevOrg = SetEnv(OpenAIConfig.VariableNames.OPENAI_ORGANIZATION, "O_ORG");

            var config = OpenAIConfig.FromEnvironment();
            Assert.False(config.Azure);
            Assert.Equal("O_ENDPOINT", config.Endpoint);
            Assert.Equal("O_API", config.ApiKey);
            Assert.Equal("O_ORG", config.Organization);
        }
        finally
        {
            SetEnv(OpenAIConfig.VariableNames.OPENAI_ENDPOINT, prevEndpoint);
            SetEnv(OpenAIConfig.VariableNames.OPENAI_API_KEY, prevKey);
            SetEnv(OpenAIConfig.VariableNames.OPENAI_ORGANIZATION, prevOrg);
        }
    }

    [Fact]
    public void TestEnvAzure()
    {
        string? prevEndpoint = null;
        string? prevKey = null;
        try
        {
            prevEndpoint = SetEnv(OpenAIConfig.VariableNames.AZURE_OPENAI_ENDPOINT, "A_ENDPOINT");
            prevKey = SetEnv(OpenAIConfig.VariableNames.AZURE_OPENAI_API_KEY, "A_API");

            var config = OpenAIConfig.FromEnvironment();
            Assert.True(config.Azure);
            Assert.Equal("A_ENDPOINT", config.Endpoint);
            Assert.Equal("A_API", config.ApiKey);
        }
        finally
        {
            SetEnv(OpenAIConfig.VariableNames.AZURE_OPENAI_ENDPOINT, prevEndpoint);
            SetEnv(OpenAIConfig.VariableNames.AZURE_OPENAI_API_KEY, prevKey);
        }
    }
}
