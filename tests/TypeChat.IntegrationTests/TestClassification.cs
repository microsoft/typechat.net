// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Tests;

public class TestClassification : TypeChatTest, IClassFixture<Config>
{
    Config _config;

    public TestClassification(Config config, ITestOutputHelper output)
        : base(output)
    {
        _config = config;
    }

    [Fact]
    public void TestRouter()
    {
        TextRequestRouter<string> router = new TextRequestRouter<string>(new MockLanguageModel());
        AddRoutes(router, out int countAdded);

        Assert.Equal(countAdded, router.Routes.Count);
        foreach (var shop in Classes.Shops())
        {
            Assert.True(router.Routes.ContainsKey(shop.Key));
        }
        Assert.False(router.Routes.ContainsKey("Foo"));
    }

    [SkippableFact]
    public async Task TestRouting()
    {
        Skip.If(!CanRunEndToEndTest(_config));
        
        TextRequestRouter<string> router = CreateRouter();
        string query = "I want to buy a Sherlock Holmes novel";
        string route = await router.RouteRequestAsync(query);
        var classify = await router.ClassifyRequestAsync(query);
        Assert.Equal("Shop2", route);
        Assert.Equal(route, classify.Value);
    }

    TextRequestRouter<string> CreateRouter()
    {
        TextRequestRouter<string> router = new TextRequestRouter<string>(new ChatLanguageModel(_config.OpenAI));
        AddRoutes(router, out _);
        return router;
    }

    string ShopId(int i) => $"Shop{i}";

    void AddRoutes(TextRequestRouter<string> router, out int numRoutes)
    {
        int i = 0;
        foreach (var shop in Classes.Shops())
        {
            ++i;
            router.Add(ShopId(i), shop.Key, shop.Value);
        }
        numRoutes = i;
    }
}
