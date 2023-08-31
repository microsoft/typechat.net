﻿// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Tests;

public class TestEndToEnd : TypeChatTest, IClassFixture<Config>
{
    Config _config;

    public TestEndToEnd(Config config)
    {
        _config = config;
    }

    [Fact]
    public async Task Translate()
    {
        if (!CanRunEndToEndTest(_config))
        {
            Trace.WriteLine("No Open AI. Skipping");
            return;
        }

        var translator = new JsonTranslator<SentimentResponse>(
            new LanguageModel(_config.OpenAI),
            new TypeValidator<SentimentResponse>()
        );
        SentimentResponse response = await translator.TranslateAsync("Tonights gonna be a good night! A good good night!");
        Assert.NotNull(response);
        Assert.NotNull(response.Sentiment);

        response = await translator.TranslateAsync("Its been a long days night, and I've been working like a dog");
        Assert.NotNull(response);
        Assert.NotNull(response.Sentiment);
    }

    /// <summary>
    /// This one loads the schema from a TS file
    [Fact]
    public async Task TranslateWithTSFileSchema()
    {
        if (!CanRunEndToEndTest(_config))
        {
            Trace.WriteLine("No Open AI. Skipping");
            return;
        }

        SchemaText schema = SchemaText.Load("./SentimentSchema.ts");
        var translator = new JsonTranslator<SentimentResponse>(
            new LanguageModel(_config.OpenAI),
            schema
        );
        SentimentResponse response = await translator.TranslateAsync("Tonights gonna be a good night! A good good night!");
        Assert.NotNull(response);
        Assert.NotNull(response.Sentiment);

        response = await translator.TranslateAsync("Its been a long days night, and I've been working like a dog");
        Assert.NotNull(response);
        Assert.NotNull(response.Sentiment);
    }

    [Fact]
    public async Task ProgramMath()
    {
        if (!CanRunEndToEndTest(_config))
        {
            Trace.WriteLine("No Open AI. Skipping");
            return;
        }

        string request = "3 * 5 + 2 * 7";
        double expectedResult = 29.0;

        Api<IMathAPI> api = MathAPI.Default;
        var translator = new ProgramTranslator<IMathAPI>(new LanguageModel(_config.OpenAI), api);
        var program = await translator.TranslateAsync(request);
        Assert.NotNull(program);
        Assert.True(program.IsComplete);
        dynamic result = program.Run(api);
        Assert.True(result == expectedResult);
    }
}