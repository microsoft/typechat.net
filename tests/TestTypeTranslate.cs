// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Tests;

public class TestTypeTranslate : TypeChatTest, IClassFixture<Config>
{
    Config _config;

    public TestTypeTranslate(Config config)
    {
        _config = config;
    }

    [Fact]
    public async Task TestEndToEnd()
    {
        if (!CanRunEndToEndTest(_config))
        {
            Trace.WriteLine("No Open AI. Skipping");
            return;
        }

        var service = new JsonTranslator<SentimentResponse>(
            new CompletionService(_config.OpenAI),
            new TypeValidator<SentimentResponse>()
        );
        SentimentResponse response = await service.TranslateAsync("Tonights gonna be a good night! A good good night!");
        Assert.NotNull(response);
        Assert.NotNull(response.Sentiment);

        response = await service.TranslateAsync("Its been a long days night, and I've been working like a dog");
        Assert.NotNull(response);
        Assert.NotNull(response.Sentiment);
    }

    /// <summary>
    /// This one loads the schema from a TS file
    [Fact]
    public async Task TestEndToEnd_Raw()
    {
        if (!CanRunEndToEndTest(_config))
        {
            Trace.WriteLine("No Open AI. Skipping");
            return;
        }

        SchemaText schema = SchemaText.Load("./SentimentSchema.ts");
        var service = new JsonTranslator<SentimentResponse>(
            new CompletionService(_config.OpenAI),
            schema
        );
        SentimentResponse response = await service.TranslateAsync("Tonights gonna be a good night! A good good night!");
        Assert.NotNull(response);
        Assert.NotNull(response.Sentiment);

        response = await service.TranslateAsync("Its been a long days night, and I've been working like a dog");
        Assert.NotNull(response);
        Assert.NotNull(response.Sentiment);
    }
}
