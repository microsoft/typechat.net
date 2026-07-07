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
        await RunRetry(config);

        config.MaxPauseMs = 0;
        await RunRetry(config);

        config.MaxRetries = 0;
        await RunRetry(config);
    }

    private async Task RunRetry(OpenAIConfig config)
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
        string modelResponse = await model.CompleteAsync("Hello");
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

    private (string, string) CannedResponse()
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

    [Fact]
    public async Task TestCompletionInfo()
    {
        var config = MockOpenAIConfig();
        var (jsonResponse, expected) = CannedResponse();
        var handler = new MockHttpHandler(jsonResponse);
        using LanguageModel model = new LanguageModel(config, null, new HttpClient(handler));

        LanguageModelResponse response = await model.CompleteAsync("Hello");

        Assert.Equal(expected, response.Text.Trim());
        Assert.NotNull(response.Info);
        Assert.Equal("gpt-3.5-turbo-0613", response.Info.Model);
        Assert.Equal(CompletionFinishReason.Stop, response.Info.FinishReason);
        Assert.NotNull(response.Info.Usage);
        Assert.Equal(9, response.Info.Usage.PromptTokens);
        Assert.Equal(12, response.Info.Usage.CompletionTokens);
        Assert.Equal(21, response.Info.Usage.TotalTokens);
        Assert.NotNull(response.Info.Raw);
        Assert.Equal("chatcmpl-123", response.Info.Raw.Value.GetProperty("id").GetString());
    }

    [Fact]
    public async Task TestResponsesApi()
    {
        OpenAIConfig config = MockOpenAIConfig(azure: false);
        config.Endpoint = "https://api.openai.com/v1/responses";
        const string jsonResponse = @"{
          ""id"": ""resp-123"",
          ""object"": ""response"",
          ""status"": ""completed"",
          ""model"": ""gpt-4.1-2025-04-14"",
          ""output"": [{
            ""type"": ""message"",
            ""role"": ""assistant"",
            ""content"": [{ ""type"": ""output_text"", ""text"": ""Hi there!"" }]
          }],
          ""usage"": { ""input_tokens"": 20, ""output_tokens"": 5, ""total_tokens"": 25 }
        }";
        var handler = new MockHttpHandler(jsonResponse);
        using LanguageModel model = new LanguageModel(config, null, new HttpClient(handler));

        LanguageModelResponse response = await model.CompleteAsync("Hello");

        // Routed to the /responses endpoint.
        Assert.Equal("https://api.openai.com/v1/responses", handler.LastRequest.RequestUri.AbsoluteUri);
        // Text + normalized info parsed from the Responses response shape.
        Assert.Equal("Hi there!", response.Text);
        Assert.Equal(CompletionFinishReason.Stop, response.Info.FinishReason);
        Assert.NotNull(response.Info.Usage);
        Assert.Equal(20, response.Info.Usage.PromptTokens);
        Assert.Equal(5, response.Info.Usage.CompletionTokens);
        Assert.Equal(25, response.Info.Usage.TotalTokens);
    }

    [Fact]
    public void Test_Prompt()
    {
        Prompt prompt = new Prompt();
        prompt.AppendInstruction("Help the user translate approximate date ranges into precise ones");
        prompt.Add(PromptLibrary.Now());
        prompt.AppendResponse("OK, thank you");

        Assert.Equal(PromptSection.Sources.System, prompt[0].Source);
        Assert.Equal(PromptSection.Sources.User, prompt[1].Source);
        Assert.Equal(PromptSection.Sources.Assistant, prompt[2].Source);

    }

}
