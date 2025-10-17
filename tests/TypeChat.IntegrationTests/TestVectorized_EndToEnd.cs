// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Tests;

public class TestVectorized_EndToEnd : TypeChatTest, IClassFixture<Config>
{
    private readonly Config _config;

    public TestVectorized_EndToEnd(Config config, ITestOutputHelper output)
        : base(output)
    {
        _config = config;
    }

    [SkippableFact]
    public async Task TestEmbeddings()
    {
        Skip.If(!CanRunEndToEndTest_Embeddings(_config));

        //
        // Get the same embedding twice. Should have same result
        //
        string request = "The area of a circle is Pi R Squared";
        TextEmbeddingModel model = new TextEmbeddingModel(_config.OpenAIEmbeddings);
        Embedding embeddingX = new Embedding(await model.GenerateEmbeddingAsync(request));
        Assert.True(embeddingX.Size > 0);
        Embedding embeddingY = new Embedding(await model.GenerateEmbeddingAsync(request));
        Assert.True(embeddingY.Size > 0);
        double score = embeddingX.CosineSimilarity(embeddingY);
        Assert.Equal(1, score.RoundToInt()); // Round up. Cosine should be 1 - identical embeddings

        string[] requests = new string[]
        {
            "The circumference of a circle is 2 Pi R",
            "All we are, is dust in the wind, dude"
        };
        Microsoft.Extensions.AI.Embedding<float>[] embeddings_float = await model.GenerateEmbeddingsAsync(requests);
        Embedding[] embeddings = new Embedding[embeddings_float.Length];
        for (int i = 0; i < embeddings_float.Length; i++)
        {
            embeddings[i] = new Embedding(embeddings_float[i]);
        }
        Assert.Equal(2, embeddings.Length);
        double score1 = embeddingX.CosineSimilarity(embeddings[0]);
        double score2 = embeddingX.CosineSimilarity(embeddings[1]);
        // The first score should be higher than the second
        Assert.True(score1 > score2);
    }

    [SkippableFact]
    public async Task TestVectorTextIndex()
    {
        Skip.If(!CanRunEndToEndTest_Embeddings(_config));

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

    [SkippableFact]
    public async Task TestRouting()
    {
        Skip.If(!CanRunEndToEndTest_Embeddings(_config));

        VectorTextIndex<string> index = CreateIndex<string>();
        foreach (var shop in Classes.Shops())
        {
            await index.AddAsync(shop.Key, shop.Value);
        }
        string match = await index.RouteRequestAsync("Want to buy a book about artificial intelligence");
        Assert.True(index.Items.Contains(match));
    }

    [SkippableFact]
    public async Task TestMessageIndex()
    {
        Skip.If(!CanRunEndToEndTest_Embeddings(_config));

        VectorizedMessageList messages = new VectorizedMessageList(new TextEmbeddingModel(_config.OpenAIEmbeddings));
        Assert.Equal(VectorizedMessageList.DefaultMaxMatches, messages.MaxContextMatches);

        messages.MaxContextMatches = 2;
        string[] desserts = new string[]
        {
            "Strawberry Shortcake, Tiramisu",
            "Donuts and Eclairs",
        };
        string[] books = new string[]
        {
            "Popular Science, Biographies, Textbooks",
            "Mysteries, Thrillers, Science Fiction",
        };
        for (int i = 0; i < books.Length; ++i)
        {
            messages.Append(books[i]);
        }
        for (int i = 0; i < desserts.Length; ++i)
        {
            await messages.AppendAsync(desserts[i]);
        }
        await foreach (var match in messages.GetContextAsync("Chocolate Cake"))
        {
            Assert.True(desserts.Contains(match.GetText()));
        }
        await foreach (var match in messages.GetContextAsync("Literary fiction"))
        {
            Assert.True(books.Contains(match.GetText()));
        }
    }

    private VectorTextIndex<T> CreateIndex<T>()
    {
        return new VectorTextIndex<T>(new TextEmbeddingModel(_config.OpenAIEmbeddings));
    }
}
