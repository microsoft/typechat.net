// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Tests;

/// <summary>
/// Online (end to end) tests for multimodal (image) prompts.
///
/// These use the built-in <see cref="LanguageModel"/> (which supports image content) and require a
/// vision capable model such as gpt-4o. The CI pipeline is configured with a gpt-4o deployment.
/// The Microsoft logo image ships with the test project (see the csproj) and is copied next to the
/// test binaries.
/// </summary>
public class TestMultimodalEndToEnd : TypeChatTest, IClassFixture<Config>
{
    // Image (vision) input on Azure OpenAI requires a recent api-version. The default OpenAIConfig
    // api-version predates vision, so use a vision capable version for these tests.
    private const string VisionApiVersion = "2024-08-01-preview";
    // The OpenAI Responses API requires a recent (preview) api-version on Azure.
    private const string ResponsesApiVersion = "2025-04-01-preview";
    private const string LogoImagePath = "microsoft-logo.png";

    private readonly Config _config;

    public TestMultimodalEndToEnd(Config config, ITestOutputHelper output)
        : base(output)
    {
        _config = config;
    }

    [SkippableFact]
    public async Task DescribeMicrosoftLogo_FromFile()
    {
        Skip.If(!CanRunEndToEndTest(_config));

        LogoInfo info = await DescribeImage(PromptImage.FromFile(LogoImagePath, "image/png", ImageDetail.High));

        Assert.NotNull(info);
        WriteLine($"Brand='{info.Brand}' ContainsText={info.ContainsText} Colors=[{string.Join(", ", info.Colors ?? Array.Empty<string>())}]");

        // The Microsoft logo contains the word "Microsoft".
        Assert.True(info.ContainsText);
        Assert.False(string.IsNullOrEmpty(info.Brand));
        Assert.Contains("microsoft", info.Brand, StringComparison.OrdinalIgnoreCase);
    }

    [SkippableFact]
    public async Task TranslateSentiment_ResponsesApi()
    {
        Skip.If(!CanRunEndToEndTest(_config));

        LanguageModel model = new LanguageModel(ResponsesConfig(_config.OpenAI));
        JsonTranslator<SentimentResponse> translator = new JsonTranslator<SentimentResponse>(
            model,
            new TypeValidator<SentimentResponse>()
        );

        SentimentResponse response = await translator.TranslateAsync("Tonights gonna be a good night! A good good night!");
        Assert.NotNull(response);
        Assert.False(string.IsNullOrEmpty(response.Sentiment));
        WriteLine($"[Responses API] Sentiment='{response.Sentiment}'");
    }

    private async Task<LogoInfo> DescribeImage(PromptImage image)
    {
        LanguageModel model = new LanguageModel(VisionConfig(_config.OpenAI));
        JsonTranslator<LogoInfo> translator = new JsonTranslator<LogoInfo>(model);

        Prompt request = new Prompt();
        request.Add(new MultimodalPromptSection()
            .AddText("Describe the following image.")
            .AddImage(image));

        return await translator.TranslateAsync(request, null);
    }

    /// <summary>
    /// Copy the shared fixture config (so we don't mutate it) and make it suitable for image input:
    /// a vision capable Azure api-version and a longer timeout, since vision calls can be slower.
    /// </summary>
    private static OpenAIConfig VisionConfig(OpenAIConfig src)
    {
        return new OpenAIConfig
        {
            Azure = src.Azure,
            Endpoint = src.Endpoint,
            ApiKey = src.ApiKey,
            Organization = src.Organization,
            Model = src.Model,
            ApiVersion = src.Azure ? VisionApiVersion : src.ApiVersion,
            TimeoutMs = Math.Max(src.TimeoutMs, 60 * 1000),
            MaxRetries = src.MaxRetries,
            MaxPauseMs = src.MaxPauseMs,
            ApiTokenProvider = src.ApiTokenProvider,
            UseResponsesApi = src.UseResponsesApi,
        };
    }

    /// <summary>
    /// Copy the shared fixture config and switch it to the OpenAI Responses API with an api-version
    /// that supports it on Azure.
    /// </summary>
    private static OpenAIConfig ResponsesConfig(OpenAIConfig src)
    {
        return new OpenAIConfig
        {
            Azure = src.Azure,
            Endpoint = src.Endpoint,
            ApiKey = src.ApiKey,
            Organization = src.Organization,
            Model = src.Model,
            ApiVersion = src.Azure ? ResponsesApiVersion : src.ApiVersion,
            TimeoutMs = Math.Max(src.TimeoutMs, 60 * 1000),
            MaxRetries = src.MaxRetries,
            MaxPauseMs = src.MaxPauseMs,
            ApiTokenProvider = src.ApiTokenProvider,
            UseResponsesApi = true,
        };
    }
}

/// <summary>
/// A strongly typed description of a logo image.
/// </summary>
public class LogoInfo
{
    [Comment("The name of the company or brand shown in the image, if identifiable")]
    public string Brand { get; set; }

    [Comment("True if the image contains any text")]
    public bool ContainsText { get; set; }

    [Comment("The dominant colors in the image")]
    public string[] Colors { get; set; }
}
