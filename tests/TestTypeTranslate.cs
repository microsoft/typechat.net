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
        if (!_config.HasOpenAI)
        {
            Trace.WriteLine("No Open AI. Skipping");
            return;
        }

        SchemaText schema = SchemaText.Load("./SentimentSchema.ts");
        var service = KernelFactory.JsonTranslator<SentimentResponse>(
            schema,
            Config.ModelNames.Gpt35Turbo,
            _config.OpenAI
        );
        SentimentResponse response = await service.TranslateAsync("Tonights gonna be a good night! A good good night!");
        Assert.NotNull(response);
        Assert.NotNull(response.Sentiment);

        response = await service.TranslateAsync("Its been a long days night, and I've been working like a dog");
        Assert.NotNull(response);
        Assert.NotNull(response.Sentiment);
    }
}
