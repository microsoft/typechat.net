// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Tests;

public enum Coffees
{
    [Comment("Drip coffee")]
    Coffee,
    Latte,
    Mocha,
    [Comment("For all other coffee types")]
    Unknown,
}

public enum CoffeeSize
{
    Small,
    Medium,
    Large,
    Grande,
    Venti
}

[Comment("Orders for coffee only")]
public class CoffeeOrder
{
    [JsonPropertyName("coffee")]
    public Coffees Coffee { get; set; }
    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }
    [JsonPropertyName("size")]
    public CoffeeSize Size { get; set; }
}

public class DessertOrder
{
    [Vocab("Desserts")]
    [JsonPropertyName("dessert")]
    public string Name { get; set; }
    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }
}

public class FruitOrder
{
    [Vocab("Fruits")]
    [JsonPropertyName("fruit")]
    public string Name { get; set; }
    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }
}

public class Order
{
    [JsonPropertyName("coffee")]
    public CoffeeOrder[] Coffees { get; set; }
    [JsonPropertyName("desserts")]
    public DessertOrder[] Desserts { get; set; }
    [JsonPropertyName("fruits")]
    public FruitOrder[] Fruits { get; set; }
}

public class SentimentResponse
{
    [JsonPropertyName("sentiment")]
    public string Sentiment { get; set; }
}

public static class TestVocabs
{
    public static VocabType Desserts()
    {
        Vocab vocab = new Vocab
        {
            "Tiramisu",
            "Strawberry Cake",
            "Banana Cake",
            "Coffee Cake",
            "Strawberry Shortcake",
            "Chocolate Cake"
        };
        return new VocabType("Desserts", vocab);
    }

    public static VocabType Fruits()
    {
        Vocab vocab = new Vocab
        {
            "Regular Apple",
            "Honeycrisp Apple",
            "Banana",
            "Rainier Cherry",
            "Blackberry",
            "Pear",
            "Nectarine"
        };

        return new VocabType("Fruits", vocab);
    }
}
