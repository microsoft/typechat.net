// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Tests;

public class TestAgent : TypeChatTest, IClassFixture<Config>
{
    Config _config;
    VocabType _desserts;

    public TestAgent(Config config)
    {
        _config = config;
    }

    public IVocab Desserts
    {
        get
        {
            _desserts ??= TestVocabs.Desserts();
            return _desserts.Vocab;
        }
    }

    [Fact]
    public async Task TestEndToEnd()
    {
        if (!CanRunEndToEndTest(_config))
        {
            return;
        }

        Agent<Order> agent = new Agent<Order>(_config.CreateTranslator<Order>());
        agent.ResponseToMessage = (r) => null; // Don't save responses

        Order order = await agent.TranslateAsync("I would like 1 strawberry shortcake");
        Validate(order.Desserts, "Strawberry Shortcake");

        agent.Preamble.Push("The following are PAST requests from the user. Use them during translation.");

        order = await agent.TranslateAsync("Actually, no shortcake. Make it 2 tiramisu instead");
        Validate(order.Desserts, new DessertOrder("Tiramisu", 2));

        order = await agent.TranslateAsync("And you know, how about adding a strawbery cake");
        Validate(order.Desserts, new DessertOrder("Tiramisu", 2), "Strawberry Cake");
    }

    void Validate(DessertOrder[] order, params DessertOrder[] expected)
    {
        Assert.True(order.Length == expected.Length);
        for (int i = 0; i < order.Length; ++i)
        {
            Validate(order[i], expected[i]);
        }
    }

    void Validate(DessertOrder order, DessertOrder expected)
    {
        Assert.Equal(order.Quantity, expected.Quantity);
        Assert.Equal(order.Name, expected.Name);
        Assert.True(Desserts.Contains(order.Name));
    }
}
