// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Tests;

public class TestVectorized_EndToEnd : TypeChatTest, IClassFixture<Config>
{
    Config _config;

    public TestVectorized_EndToEnd(Config config)
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
        //
        // Get the same embedding twice. Should have same result
        //
        string request = "The area of a circle is Pi R Squared";
        TextEmbeddingModel model = new TextEmbeddingModel(_config.OpenAIEmbeddings);
        Embedding embeddingX = await model.GenerateEmbeddingAsync(request);
        Assert.True(embeddingX.Size > 0);
        Embedding embeddingY = await model.GenerateEmbeddingAsync(request);
        Assert.True(embeddingY.Size > 0);
        double score = embeddingX.CosineSimilarity(embeddingY);
        Assert.Equal(1, score.RoundToInt()); // Round up. Cosine should be 1 - identical embeddings

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

    [Fact]
    public async Task TestVectorTextIndex()
    {
        if (!CanRunEndToEndTest_Embeddings(_config, nameof(TestEmbeddings)))
        {
            return;
        }

        VectorTextIndex<int> index = CreateIndex<int>();

        const int numItems = 10; // Total # of items to put in the index
        const int numArray = 5;  // Number to add using array/batch methods
        const int numSingle = numItems - numArray; // Number to add using Add alone
        const int testItem = 6; // Which item to search for

        int i = 0;
        // Each item is insert with a string pattern - This is the number
        // Since add
        for (; i < numSingle; ++i)
        {
            await index.AddAsync(i, $"This is the number {i}");
        }
        // Batch add
        int[] intValues = new int[numArray];
        string[] stringValues = new string[numArray];
        for (; i < numItems; ++i)
        {
            intValues[i - numSingle] = i;
            stringValues[i - numSingle] = $"This is the number {i}";
        }
        await index.AddAsync(intValues, stringValues);
        // Ensure they all got added
        Assert.Equal(numItems, index.Items.Count);
        //
        // Search for the closest match
        //
        int iMatch = await index.NearestAsync($"Number {testItem}");
        Assert.Equal(testItem, iMatch);

        var matches = await index.NearestAsync($"Number {testItem}", 5);
        Assert.Equal(5, matches.Count);
        Assert.Equal(iMatch, matches[0]);
    }

    [Fact]
    public async Task TestRouting()
    {
        if (!CanRunEndToEndTest_Embeddings(_config, nameof(TestEmbeddings)))
        {
            return;
        }

        VectorTextIndex<string> index = CreateIndex<string>();
        foreach (var shop in Classes.Shops())
        {
            await index.AddAsync(shop.Key, shop.Value);
        }
        string match = await index.RouteRequestAsync("Want to buy a book about artificial intelligence");
        Assert.True(index.Items.Contains(match));
    }

    VectorTextIndex<T> CreateIndex<T>()
    {
        return new VectorTextIndex<T>(new TextEmbeddingModel(_config.OpenAIEmbeddings));
    }
}
