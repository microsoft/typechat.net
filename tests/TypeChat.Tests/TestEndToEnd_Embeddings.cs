// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Tests;

public class TestEndToEnd_Embeddings : TypeChatTest, IClassFixture<Config>
{
    Config _config;

    public TestEndToEnd_Embeddings(Config config)
    {
        _config = config;
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
        Assert.Equal(1, score.RoundToInt()); // Round up

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
