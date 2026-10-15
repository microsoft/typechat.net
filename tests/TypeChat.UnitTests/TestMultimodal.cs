// Copyright (c) Microsoft. All rights reserved.

extern alias ExtensionsAI;
using System.Text.Json;
using System.Threading;
using Microsoft.Extensions.AI;
using Microsoft.TypeChat.Tests;
using MeaiChatModel = ExtensionsAI::Microsoft.TypeChat.ChatLanguageModel;

namespace Microsoft.TypeChat.UnitTests;

public class TestMultimodal : TypeChatTest
{
    #region Content types

    [Fact]
    public void PromptImage_FromBytes_ProducesDataUri()
    {
        byte[] bytes = new byte[] { 1, 2, 3, 4 };
        PromptImage image = PromptImage.FromBytes(bytes, "image/png", ImageDetail.Low);

        Assert.True(image.IsDataUri);
        Assert.Equal(ImageDetail.Low, image.Detail);
        Assert.StartsWith("data:image/png;base64,", image.Url);
        Assert.Equal(Convert.ToBase64String(bytes), image.Url.Substring("data:image/png;base64,".Length));
    }

    [Fact]
    public void PromptImage_Url_IsNotDataUri()
    {
        PromptImage image = new PromptImage("https://example.com/cat.png");
        Assert.False(image.IsDataUri);
        Assert.Equal(ImageDetail.Auto, image.Detail);
    }

    [Fact]
    public void PromptImage_FromFile_LoadsMicrosoftLogo()
    {
        // The shared Microsoft logo asset is copied next to the test binaries (see csproj).
        PromptImage image = PromptImage.FromFile("microsoft-logo.png");
        Assert.True(image.IsDataUri);
        Assert.StartsWith("data:image/png;base64,", image.Url);
        Assert.True(image.Url.Length > 1000); // real image bytes were embedded
    }

    [Theory]
    [InlineData("cat.png", "image/png")]
    [InlineData("cat.PNG", "image/png")]
    [InlineData("cat.jpg", "image/jpeg")]
    [InlineData("cat.jpeg", "image/jpeg")]
    [InlineData("cat.gif", "image/gif")]
    [InlineData("cat.webp", "image/webp")]
    [InlineData("https://example.com/photos/cat.bmp", "image/bmp")]
    [InlineData("cat", "image/*")]
    [InlineData("https://example.com/image", "image/*")]
    public void PromptImage_GetMediaType(string pathOrUrl, string expected)
    {
        Assert.Equal(expected, PromptImage.GetMediaType(pathOrUrl));
    }

    [Fact]
    public void PromptContentPart_ImplicitConversions()
    {
        PromptContentPart text = "hello";
        Assert.True(text.IsText);
        Assert.False(text.IsImage);
        Assert.Equal("hello", text.Text);

        PromptContentPart image = new PromptImage("https://example.com/cat.png");
        Assert.True(image.IsImage);
        Assert.False(image.IsText);
        Assert.NotNull(image.Image);
    }

    [Fact]
    public void MultimodalSection_BuildsOrderedParts()
    {
        MultimodalPromptSection section = new MultimodalPromptSection()
            .AddText("a")
            .AddImage("https://example.com/cat.png", ImageDetail.High)
            .AddText("b");

        Assert.Equal(PromptSection.Sources.User, section.Source);
        Assert.True(section.HasImages);
        Assert.Equal(3, section.ContentParts.Count);
        Assert.True(section.ContentParts[0].IsText);
        Assert.True(section.ContentParts[1].IsImage);
        Assert.Equal(ImageDetail.High, section.ContentParts[1].Image!.Detail);
        Assert.True(section.ContentParts[2].IsText);

        // GetText concatenates text parts and ignores images, so text-only models still work.
        Assert.Equal("a\nb", section.GetText());
    }

    [Fact]
    public void MultimodalSection_TextOnly_HasNoImages()
    {
        MultimodalPromptSection section = new MultimodalPromptSection().AddText("just text");
        Assert.False(section.HasImages);
        Assert.Equal("just text", section.GetText());
    }

    #endregion

    #region LanguageModel - Chat Completions wire format

    [Fact]
    public async Task LanguageModel_TextOnly_SendsStringContent()
    {
        OpenAIConfig config = MockOpenAIConfig(azure: false);
        MockHttpHandler handler = new MockHttpHandler(ChatCompletionsCanned());
        using LanguageModel model = new LanguageModel(config, null, new HttpClient(handler));

        string text = await model.CompleteAsync("Hello");
        Assert.Equal("Hello there!", text.Trim());

        JsonNode body = ParseBody(handler);
        JsonArray messages = body["messages"]!.AsArray();
        Assert.Single(messages);
        JsonNode content = messages[0]!["content"]!;
        Assert.Equal(JsonValueKind.String, content.GetValueKind());
        Assert.Equal("Hello", content.GetValue<string>());
        Assert.Null(body["input"]);
    }

    [Fact]
    public async Task LanguageModel_Multimodal_SendsContentParts()
    {
        OpenAIConfig config = MockOpenAIConfig(azure: false);
        config.Model = "gpt-4o";
        MockHttpHandler handler = new MockHttpHandler(ChatCompletionsCanned());
        using LanguageModel model = new LanguageModel(config, null, new HttpClient(handler));

        Prompt prompt = new Prompt();
        prompt.Add(new MultimodalPromptSection()
            .AddText("What is in this image?")
            .AddImage("https://example.com/cat.png", ImageDetail.High));

        string text = await model.CompleteAsync(prompt);
        Assert.Equal("Hello there!", text.Trim());

        JsonNode body = ParseBody(handler);
        JsonNode content = body["messages"]!.AsArray()[0]!["content"]!;
        Assert.Equal(JsonValueKind.Array, content.GetValueKind());

        JsonArray parts = content.AsArray();
        Assert.Equal(2, parts.Count);
        Assert.Equal("text", parts[0]!["type"]!.GetValue<string>());
        Assert.Equal("What is in this image?", parts[0]!["text"]!.GetValue<string>());
        Assert.Equal("image_url", parts[1]!["type"]!.GetValue<string>());
        Assert.Equal("https://example.com/cat.png", parts[1]!["image_url"]!["url"]!.GetValue<string>());
        Assert.Equal("high", parts[1]!["image_url"]!["detail"]!.GetValue<string>());
    }

    [Fact]
    public async Task LanguageModel_Multimodal_AutoDetail_IsOmitted()
    {
        OpenAIConfig config = MockOpenAIConfig(azure: false);
        MockHttpHandler handler = new MockHttpHandler(ChatCompletionsCanned());
        using LanguageModel model = new LanguageModel(config, null, new HttpClient(handler));

        Prompt prompt = new Prompt();
        prompt.Add(new MultimodalPromptSection().AddImage("https://example.com/cat.png")); // Auto

        await model.CompleteAsync(prompt);

        JsonNode body = ParseBody(handler);
        JsonNode imageUrl = body["messages"]!.AsArray()[0]!["content"]!.AsArray()[0]!["image_url"]!;
        Assert.Null(imageUrl["detail"]);
    }

    #endregion

    #region LanguageModel - Responses API

    [Fact]
    public async Task LanguageModel_ResponsesEndpoint_UsesInputAndParsesOutput()
    {
        OpenAIConfig config = MockOpenAIConfig(azure: false);
        config.Endpoint = "https://api.openai.com/v1/responses";
        config.Model = "gpt-4o";
        MockHttpHandler handler = new MockHttpHandler(ResponsesCanned());
        using LanguageModel model = new LanguageModel(config, null, new HttpClient(handler));

        string text = await model.CompleteAsync("Hello");
        Assert.Equal("Hello there!", text.Trim());

        JsonNode body = ParseBody(handler);
        Assert.Null(body["messages"]);
        Assert.NotNull(body["input"]);
        Assert.Equal("gpt-4o", body["model"]!.GetValue<string>());

        JsonArray input = body["input"]!.AsArray();
        Assert.Equal("user", input[0]!["role"]!.GetValue<string>());
        Assert.Equal("Hello", input[0]!["content"]!.GetValue<string>());
    }

    [Fact]
    public async Task LanguageModel_UseResponsesApiTrue_OverridesChatCompletionsEndpoint()
    {
        OpenAIConfig config = MockOpenAIConfig(azure: false); // endpoint targets /chat/completions
        config.UseResponsesApi = true;
        MockHttpHandler handler = new MockHttpHandler(ResponsesCanned());
        using LanguageModel model = new LanguageModel(config, null, new HttpClient(handler));

        string text = await model.CompleteAsync("Hello");
        Assert.Equal("Hello there!", text.Trim());

        JsonNode body = ParseBody(handler);
        Assert.NotNull(body["input"]);
        Assert.Null(body["messages"]);
    }

    [Fact]
    public async Task LanguageModel_UseResponsesApiFalse_OverridesResponsesEndpoint()
    {
        OpenAIConfig config = MockOpenAIConfig(azure: false);
        config.Endpoint = "https://api.openai.com/v1/responses";
        config.UseResponsesApi = false;
        MockHttpHandler handler = new MockHttpHandler(ChatCompletionsCanned());
        using LanguageModel model = new LanguageModel(config, null, new HttpClient(handler));

        string text = await model.CompleteAsync("Hello");
        Assert.Equal("Hello there!", text.Trim());

        JsonNode body = ParseBody(handler);
        Assert.NotNull(body["messages"]);
        Assert.Null(body["input"]);
    }

    [Fact]
    public async Task LanguageModel_Azure_Responses_RoutesToResponsesUrl()
    {
        OpenAIConfig config = MockOpenAIConfig(azure: true); // base Azure resource url
        config.UseResponsesApi = true;
        MockHttpHandler handler = new MockHttpHandler(ResponsesCanned());
        using LanguageModel model = new LanguageModel(config, null, new HttpClient(handler));

        await model.CompleteAsync("Hello");

        string url = handler.LastRequest!.RequestUri!.AbsoluteUri;
        Assert.Contains("/openai/responses", url, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("api-version=", url, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("chat/completions", url, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LanguageModel_Azure_Responses_NormalizesChatCompletionsUrl()
    {
        OpenAIConfig config = MockOpenAIConfig(azure: true);
        config.Endpoint = "https://res.openai.azure.com/openai/deployments/gpt-4o/chat/completions?api-version=2024-08-01-preview";
        config.UseResponsesApi = true;
        MockHttpHandler handler = new MockHttpHandler(ResponsesCanned());
        using LanguageModel model = new LanguageModel(config, null, new HttpClient(handler));

        await model.CompleteAsync("Hello");

        string url = handler.LastRequest!.RequestUri!.AbsoluteUri;
        Assert.Contains("/openai/responses", url, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("chat/completions", url, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("deployments", url, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LanguageModel_OpenAI_Responses_NormalizesChatCompletionsUrl()
    {
        OpenAIConfig config = MockOpenAIConfig(azure: false); // endpoint targets /v1/chat/completions
        config.UseResponsesApi = true;
        MockHttpHandler handler = new MockHttpHandler(ResponsesCanned());
        using LanguageModel model = new LanguageModel(config, null, new HttpClient(handler));

        await model.CompleteAsync("Hello");

        string url = handler.LastRequest!.RequestUri!.AbsoluteUri;
        Assert.EndsWith("/v1/responses", url, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region JsonTranslator integration

    [Fact]
    public void JsonTranslatorPrompts_RequestSection_PreservesImages()
    {
        MultimodalPromptSection request = new MultimodalPromptSection()
            .AddText("Describe this")
            .AddImage("https://example.com/cat.png");

        IPromptSection section = JsonTranslatorPrompts.RequestSection(request);
        MultimodalPromptSection multimodal = Assert.IsType<MultimodalPromptSection>(section);
        Assert.True(multimodal.HasImages);
    }

    [Fact]
    public void JsonTranslatorPrompts_RequestSection_TextOnly_StaysText()
    {
        IPromptSection section = JsonTranslatorPrompts.RequestSection(PromptSection.FromUser("hello"));
        Assert.IsType<PromptSection>(section);
    }

    [Fact]
    public async Task JsonTranslator_Multimodal_SendsImageToModel()
    {
        OpenAIConfig config = MockOpenAIConfig(azure: false);
        config.Model = "gpt-4o";
        MockHttpHandler handler = new MockHttpHandler(TranslatorCanned());
        using LanguageModel model = new LanguageModel(config, null, new HttpClient(handler));
        JsonTranslator<ImageDescription> translator = new JsonTranslator<ImageDescription>(model);

        Prompt request = new Prompt();
        request.Add(new MultimodalPromptSection()
            .AddText("Describe this image")
            .AddImage("https://example.com/cat.png", ImageDetail.High));

        ImageDescription result = await translator.TranslateAsync(request, null);
        Assert.Equal("A cat", result.Description);

        // The image must have survived all the way to the wire request.
        JsonNode body = ParseBody(handler);
        JsonArray messages = body["messages"]!.AsArray();
        bool sentImage = messages.Any(m =>
            m!["content"]!.GetValueKind() == JsonValueKind.Array &&
            m["content"]!.AsArray().Any(p => p!["type"]!.GetValue<string>() == "image_url"));
        Assert.True(sentImage);
    }

    #endregion

    #region Microsoft.Extensions.AI mapping

    [Fact]
    public async Task Meai_Multimodal_MapsToAIContent()
    {
        FakeChatClient client = new FakeChatClient("Hello there!");
        MeaiChatModel model = new MeaiChatModel(client, "gpt-4o");

        Prompt prompt = new Prompt();
        prompt.Add(new MultimodalPromptSection()
            .AddText("What is this?")
            .AddImage(PromptImage.FromBytes(new byte[] { 1, 2, 3 }, "image/png"))  // -> DataContent
            .AddImage("https://example.com/cat.jpg")                                // -> UriContent
            .AddImage("https://example.com/image"));                                // -> UriContent (image/*)

        string text = await model.CompleteAsync(prompt);
        Assert.Equal("Hello there!", text);

        ChatMessage message = Assert.Single(client.LastMessages!);
        Assert.Equal(ChatRole.User, message.Role);
        Assert.Contains(message.Contents, c => c is TextContent t && t.Text == "What is this?");
        Assert.Contains(message.Contents, c => c is DataContent);
        Assert.Equal(2, message.Contents.Count(c => c is UriContent));
    }

    [Fact]
    public async Task Meai_TextOnly_MapsToText()
    {
        FakeChatClient client = new FakeChatClient("ok");
        MeaiChatModel model = new MeaiChatModel(client, "gpt-4o");

        await model.CompleteAsync("Hello");

        ChatMessage message = Assert.Single(client.LastMessages!);
        Assert.Equal(ChatRole.User, message.Role);
        Assert.Equal("Hello", message.Text);
    }

    #endregion

    #region Helpers

    private static JsonNode ParseBody(MockHttpHandler handler)
    {
        Assert.NotNull(handler.LastRequestBody);
        return JsonNode.Parse(handler.LastRequestBody!)!;
    }

    private static string ChatCompletionsCanned()
    {
        return @"{
          ""choices"": [{
            ""index"": 0,
            ""message"": { ""role"": ""assistant"", ""content"": ""Hello there!"" },
            ""finish_reason"": ""stop""
          }]
        }";
    }

    private static string ResponsesCanned()
    {
        return @"{
          ""id"": ""resp_123"",
          ""output"": [{
            ""type"": ""message"",
            ""role"": ""assistant"",
            ""content"": [{ ""type"": ""output_text"", ""text"": ""Hello there!"" }]
          }]
        }";
    }

    private static string TranslatorCanned()
    {
        // The assistant message content is itself a JSON object matching ImageDescription.
        return @"{
          ""choices"": [{
            ""message"": { ""role"": ""assistant"", ""content"": ""{\""Description\"": \""A cat\""}"" }
          }]
        }";
    }

    private sealed class FakeChatClient : IChatClient
    {
        private readonly string _responseText;

        public FakeChatClient(string responseText)
        {
            _responseText = responseText;
        }

        public IReadOnlyList<ChatMessage>? LastMessages { get; private set; }

        public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
        {
            LastMessages = messages.ToList();
            return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, _responseText)));
        }

        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public object? GetService(Type serviceType, object? serviceKey = null) => null;

        public void Dispose()
        {
        }
    }

    #endregion
}

public class ImageDescription
{
    public string Description { get; set; }
}
