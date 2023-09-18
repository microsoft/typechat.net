// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Tests;

public class TestAgent : TypeChatTest, IClassFixture<Config>
{
    Config _config;
    NamedVocab _desserts;

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
    public void TestMessageStream()
    {
        const int messageCount = 3;

        MessageList messageList = new MessageList();
        IMessageStream messages = messageList;
        for (int i = 0; i < messageCount; ++i)
        {
            messages.Append(i.ToString());
        }
        Assert.Equal(messageCount, messageList.Count);

        var newestList = messages.Newest().ToList();
        Assert.Equal(messageList.Count, newestList.Count);
        for (int i = messageCount - 1, j = 0; i >= 0; --i, ++j)
        {
            Assert.Equal(i.ToString(), newestList[j].GetText());
        }
    }

    [Fact]
    public async Task TestEndToEnd()
    {
        if (!CanRunEndToEndTest(_config))
        {
            return;
        }

        AgentWithHistory<Order> agent = new AgentWithHistory<Order>(_config.CreateTranslator<Order>());
        agent.CreateMessageForHistory = (r) => null; // Don't remember responses. 
        agent.Translator.MaxRepairAttempts = 3;

        Order order = await agent.GetResponseAsync("I would like 1 strawberry shortcake");
        Validate(order.Desserts, "Strawberry Shortcake");

        agent.Instructions.Append("The following are PAST requests from the user. Use them during translation.");

        order = await agent.GetResponseAsync("Actually, no shortcake. Make it 2 tiramisu instead");
        Validate(order.Desserts, new DessertOrder("Tiramisu", 2));
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
