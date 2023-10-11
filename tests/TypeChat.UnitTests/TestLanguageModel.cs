// Copyright (c) Microsoft. All rights reserved.

using Microsoft.TypeChat.Tests;

namespace Microsoft.TypeChat.UnitTests;

public class TestLanguageModel : TypeChatTest
{
    [Fact]
    public async Task TestRetry()
    {
        var handler = MockHttpHandler.ErrorResponder(429);
        var config = MockOpenAIConfig();
        config.MaxRetries = 2;
        config.MaxPauseMs = 0;
        await RunRetry(config);

        config.MaxRetries = 0;
        await RunRetry(config);
    }

    async Task RunRetry(OpenAIConfig config)
    {
        var handler = MockHttpHandler.ErrorResponder(429);
        using LanguageModel model = new LanguageModel(config, null, new HttpClient(handler));
        await Assert.ThrowsAnyAsync<Exception>(() => model.CompleteAsync("Hello"));
        Assert.Equal(config.MaxRetries + 1, handler.RequestCount);
    }

    [Fact]
    public async Task TestResponse()
    {
        var config = MockOpenAIConfig();
        var (jsonResponse, expected) = CannedResponse();
        var handler = new MockHttpHandler(jsonResponse);
        using LanguageModel model = new LanguageModel(config, null, new HttpClient(handler));
        var modelResponse = await model.CompleteAsync("Hello");
        Assert.Equal(expected, modelResponse.Trim());
    }

    [Fact]
    public async Task TestConfig_Azure()
    {
        OpenAIConfig config = MockOpenAIConfig();
        config.Endpoint = "https://yourresourcename.openai.azure.com/openai/deployments/deploymentid/chat/completions?api-version=";
        config.Model = "YOUR_MODEL";
        config.ApiVersion = "53";

        var (jsonResponse, expected) = CannedResponse();
        var handler = new MockHttpHandler(jsonResponse);
        LanguageModel model = new LanguageModel(config, null, new HttpClient(handler));
        await model.CompleteAsync("Hello");

        Assert.Equal(config.Endpoint.ToLower(), handler.LastRequest.RequestUri.AbsoluteUri.ToLower());

        model.Dispose();

        config.Endpoint = "https://yourresourcename.openai.azure.com/";
        model = new LanguageModel(config, null, new HttpClient(handler));
        await model.CompleteAsync("Hello");

        string requestUrl = handler.LastRequest.RequestUri.AbsoluteUri.ToLower();
        string expectedUrl = $"{config.Endpoint}openai/deployments/{config.Model}/chat/completions?api-version={config.ApiVersion}".ToLower();
        Assert.Equal(expectedUrl, requestUrl);
    }

    [Fact]
    public async Task TestConfig_OAI()
    {
        OpenAIConfig config = MockOpenAIConfig();
        config.Azure = false;
        config.Endpoint = "https://api.openai.com/v1/chat/completions";
        config.Model = "yourmodel";
        config.Organization = "yourorg";

        var (jsonResponse, expected) = CannedResponse();
        var handler = new MockHttpHandler(jsonResponse);
        using LanguageModel model = new LanguageModel(config, null, new HttpClient(handler));
        await model.CompleteAsync("Hello");

        HttpRequestMessage lastRequest = handler.LastRequest;
        lastRequest.Headers.Contains("OpenAI-Organization");
        lastRequest.Headers.Contains("Bearer");

        string requestUrl = handler.LastRequest.RequestUri.AbsoluteUri.ToLower();
        Assert.Equal(config.Endpoint.ToLower(), requestUrl);
    }

    (string, string) CannedResponse()
    {
        const string jsonResponse = @"{
          ""id"": ""chatcmpl-123"",
          ""object"": ""chat.completion"",
          ""created"": 1677652288,
          ""model"": ""gpt-3.5-turbo-0613"",
          ""choices"": [{
            ""index"": 0,
            ""message"": {
              ""role"": ""assistant"",
              ""content"": ""\n\nHello there!"",
            },
            ""finish_reason"": ""stop""
          }],
          ""usage"": {
            ""prompt_tokens"": 9,
            ""completion_tokens"": 12,
            ""total_tokens"": 21
          }
        }";
        return (jsonResponse, "Hello there!");
    }
}
