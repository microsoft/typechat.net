// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Tests;

public class TestEndToEnd : TypeChatTest, IClassFixture<Config>
{
    Config _config;

    public TestEndToEnd(Config config)
    {
        _config = config;
    }

    [Fact]
    public async Task TranslateSentiment()
    {
        if (!CanRunEndToEndTest(_config, nameof(TranslateSentiment)))
        {
            return;
        }
        await TranslateSentiment(new LanguageModel(_config.OpenAI));
    }

    [Fact]
    public async Task TranslateSentiment_CompletionModel()
    {
        if (!CanRunEndToEndTest(_config, nameof(TranslateSentiment_CompletionModel)))
        {
            Trace.WriteLine("No Open AI. Skipping");
            return;
        }
        await TranslateSentiment(new TextCompletionModel(_config.OpenAI));
    }

    async Task TranslateSentiment(ILanguageModel model)
    {
        var translator = new JsonTranslator<SentimentResponse>(
            model,
            new TypeValidator<SentimentResponse>()
        );
        SentimentResponse response = await translator.TranslateAsync("Tonights gonna be a good night! A good good night!");
        Assert.NotNull(response);
        Assert.NotNull(response.Sentiment);

        response = await translator.TranslateAsync("Its been a long days night, and I've been working like a dog");
        Assert.NotNull(response);
        Assert.NotNull(response.Sentiment);
    }

    [Fact]
    public async Task Translate_Polymorphic()
    {
        if (!CanRunEndToEndTest(_config, nameof(Translate_Polymorphic)))
        {
            return;
        }

        var translator = new JsonTranslator<Drawing>(new LanguageModel(_config.OpenAI));
        string request = "Add a circle of radius 4.5 at 30, 30 and\n" +
                         "Add a retangle at 5, 5 with height 10 and width 15";

        var canvas = await translator.TranslateAsync(request);
        Assert.True(canvas.Shapes.Length == 2);

        Circle circle = canvas.GetShape<Circle>();
        Assert.Equal(4.5, circle.Radius);
        Assert.Equal(30, circle.CenterX);
        Assert.Equal(30, circle.CenterY);

        Rectangle rect = canvas.GetShape<Rectangle>();
        Assert.Equal(5, rect.TopX);
        Assert.Equal(5, rect.TopY);
        Assert.Equal(10, rect.Height);
        Assert.Equal(15, rect.Width);
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
        await ProgramMath(new LanguageModel(_config.OpenAI));
    }

    [Fact]
    public async Task ProgramMath_Completion()
    {
        if (!CanRunEndToEndTest(_config))
        {
            Trace.WriteLine("No Open AI. Skipping");
            return;
        }
        await ProgramMath(new TextCompletionModel(_config.OpenAI));
    }

    async Task ProgramMath(ILanguageModel model)
    {
        string request = "3 * 5 + 2 * 7";
        double expectedResult = 29.0;

        Api<IMathAPI> api = MathAPI.Default;
        var translator = new ProgramTranslator<IMathAPI>(model, api);
        var program = await translator.TranslateAsync(request);
        Assert.NotNull(program);
        Assert.True(program.IsComplete);
        dynamic result = program.Run(api);
        Assert.True(result == expectedResult);
    }

    [Fact]
    public async Task TestEmbeddings()
    {
        if (!CanRunEndToEndTest_Embeddings(_config, nameof(TestEmbeddings)))
        {
            return;
        }

        string request = "The area of a circle is Pi R Squared";
        TextEmbeddingModel model = new TextEmbeddingModel(_config.OpenAIEmbeddings);
        Embedding embeddingX = await model.GenerateEmbeddingAsync(request);
        Assert.True(embeddingX.Size > 0);
        Embedding embeddingY = await model.GenerateEmbeddingAsync(request);
        Assert.True(embeddingY.Size > 0);
        double score = embeddingX.CosineSimilarity(embeddingY);
        Assert.Equal(1, (int)(score + 0.5)); // Round up

        string[] requests = new string[]
        {
            "The circumference of a circle is 2 Pi R",
            "All we are, is dust in the wind, dude"
        };
        Embedding[] embeddings = await model.GenerateEmbeddingsAsync(requests);
        Assert.Equal(2, embeddings.Length);
        double score1 = embeddingX.CosineSimilarity(embeddings[0]);
        double score2 = embeddingX.CosineSimilarity(embeddings[1]);
        // The first score should be higher than the second
        Assert.True(score1 > score2);
    }
}
