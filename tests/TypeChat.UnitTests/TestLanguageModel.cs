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
        var (jsonResponse, expected) = CannedResponse();
        var handler = new MockHttpHandler(jsonResponse);
        var config = MockOpenAIConfig();
        using LanguageModel model = new LanguageModel(config, null, new HttpClient(handler));
        var modelResponse = await model.CompleteAsync("Hello");
        Assert.Equal(expected, modelResponse.Trim());
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
