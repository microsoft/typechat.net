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

public class Order
{
    [JsonPropertyName("coffee")]
    public CoffeeOrder[] Coffees { get; set; }
}

public class SentimentResponse
{
    [JsonPropertyName("sentiment")]
    public string Sentiment { get; set; }
}

