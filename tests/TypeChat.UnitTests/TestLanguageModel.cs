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

        LanguageModelResponse response = await model.CompleteWithMetadataAsync("Hello");

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

        LanguageModelResponse response = await model.CompleteWithMetadataAsync("Hello");

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

    [Theory]
    [InlineData(true, true, "https://resource.example/", "https://resource.example/openai/responses?api-version=2025-04-01-preview")]
    [InlineData(true, true, "https://resource.example/openai/deployments/test-model/chat/completions?api-version=old", "https://resource.example/openai/responses?api-version=2025-04-01-preview")]
    [InlineData(true, true, "https://resource.example/openai/v1/chat/completions", "https://resource.example/openai/v1/responses")]
    [InlineData(true, true, "https://resource.example/openai/v1/", "https://resource.example/openai/v1/responses")]
    [InlineData(true, false, "https://resource.example/openai/responses?api-version=old", "https://resource.example/openai/deployments/test-model/chat/completions?api-version=2025-04-01-preview")]
    [InlineData(true, false, "https://resource.example/openai/v1/responses", "https://resource.example/openai/v1/chat/completions")]
    [InlineData(false, true, "https://api.example/v1/chat/completions?test=1", "https://api.example/v1/responses?test=1")]
    [InlineData(false, true, "https://api.example/v1/?test=1", "https://api.example/v1/responses?test=1")]
    [InlineData(false, false, "https://api.example/v1/responses/?test=1", "https://api.example/v1/chat/completions?test=1")]
    public async Task ApiSelectionPreservesEndpointAndRequestShape(bool azure, bool responsesApi, string endpoint, string expectedEndpoint)
    {
        var config = MockOpenAIConfig(azure);
        config.Endpoint = endpoint;
        config.Model = "test-model";
        config.ApiVersion = "2025-04-01-preview";
        config.UseResponsesApi = responsesApi;
        string json = responsesApi ?
            "{\"status\":\"completed\",\"output\":[{\"type\":\"message\",\"content\":[{\"type\":\"output_text\",\"text\":\"hello\"}]}]}" :
            "{\"choices\":[{\"message\":{\"content\":\"hello\"}}]}";
        var handler = new MockHttpHandler(json);
        using var model = new LanguageModel(config, client: new HttpClient(handler));
        var settings = new TranslationSettings { MaxTokens = 32, Temperature = 0.3 };

        Task<string> completion = model.CompleteAsync("Hello", settings);
        Assert.Equal("hello", await completion);
        Assert.Equal(expectedEndpoint, handler.LastRequest.RequestUri.AbsoluteUri);
        JsonObject body = JsonNode.Parse(handler.LastRequestBody).AsObject();
        Assert.True(body.ContainsKey(responsesApi ? "input" : "messages"));
        Assert.False(body.ContainsKey(responsesApi ? "messages" : "input"));
        Assert.Equal(32, body[responsesApi ? "max_output_tokens" : "max_tokens"].GetValue<int>());
        Assert.Equal(0.3, body["temperature"].GetValue<double>());
        bool needsModel = responsesApi || !azure || expectedEndpoint.Contains("/openai/v1/");
        Assert.Equal(needsModel ? "test-model" : null, body["model"]?.GetValue<string>());
    }

    [Fact]
    public async Task ResponsesIncludeAllTextPartsAndMessages()
    {
        var config = MockOpenAIConfig(azure: false);
        config.Endpoint = "https://api.example/v1/responses";
        const string json = """
            {
              "status": "completed",
              "output": [
                {"type": "reasoning"},
                {"type": "message", "content": [
                  {"type": "output_text", "text": "first"},
                  {"type": "refusal", "refusal": "ignored"},
                  {"type": "output_text", "text": "second"}
                ]},
                {"type": "message", "content": [{"type": "output_text", "text": "third"}]}
              ]
            }
            """;
        using var model = new LanguageModel(config, client: new HttpClient(new MockHttpHandler(json)));

        var response = await model.CompleteWithMetadataAsync("Hello");

        Assert.Equal("firstsecondthird", response.Text);
        Assert.Equal(CompletionFinishReason.Stop, response.Info.FinishReason);
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
