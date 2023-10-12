// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.TypeChat.Schema;

namespace Restaurant;

[Comment("an order from a restaurant that serves pizza, beer, and salad")]
[Comment("Correct common spelling mistakes like peperoni")]
public partial class Order
{
    [JsonPropertyName("items")]
    public OrderItem[] Items { get; set; }
}

[JsonPolymorphic]
[JsonDerivedType(typeof(UnknownItem), typeDiscriminator: nameof(UnknownItem))]
[JsonDerivedType(typeof(Pizza), typeDiscriminator: nameof(Pizza))]
[JsonDerivedType(typeof(Beer), typeDiscriminator: nameof(Beer))]
[JsonDerivedType(typeof(Salad), typeDiscriminator: nameof(Salad))]
public abstract partial class OrderItem { }

public abstract class LineItem : OrderItem
{
    [Comment("default: 1")]
    [JsonPropertyName("quantity")]
    public int Quantity { get; set; } = 1;
}

[Comment("Use this type for order items that match nothing else")]
public partial class UnknownItem : OrderItem
{
    [Comment("The text that wasn't understood")]
    [JsonPropertyName("text")]
    public string Text { get; set; }
}

public partial class Pizza : LineItem
{
    [Comment("default: large")]
    [JsonVocab(RestaurantVocabs.PizzaSize)]
    [JsonPropertyName("size")]
    public string? Size { get; set; }

    [Comment("toppings requested (examples: pepperoni, arugula)")]
    [JsonPropertyName("addedToppings")]
    public string[]? AddedToppings { get; set; }

    [Comment("toppings requested to be removed (examples: fresh garlic, anchovies)")]
    [JsonPropertyName("removedToppings")]
    public string[]? RemovedToppings { get; set; }

    [Comment("used if the requester references a pizza by name")]
    [JsonVocab(RestaurantVocabs.PizzaName)]
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public partial class Beer : LineItem
{
    [Comment("examples: Mack and Jacks, Sierra Nevada Pale Ale, Miller Lite")]
    [JsonPropertyName("kind")]
    public string Kind { get; set; }
}

public partial class Salad : LineItem
{
    [Comment("default: half")]
    [JsonVocab(RestaurantVocabs.SaladPortion)]
    [JsonPropertyName("portion")]
    public string? Portion { get; set; }

    [Comment("default: Garden")]
    [JsonVocab(RestaurantVocabs.SaladStyle)]
    [JsonPropertyName("style")]
    public string? Style { get; set; }

    [Comment("ingredients requested (examples: parmesan, croutons)")]
    [JsonPropertyName("addedIngredients")]
    public string[]? AddedIngredients { get; set; }

    [Comment("ingredients requested to be removed (example: red onions)")]
    [JsonPropertyName("removedIngredients")]
    public string[]? RemovedIngredients { get; set; }
}

public static class RestaurantVocabs
{
    public const string PizzaSize = "small | medium | large | extra large";
    public const string PizzaName = "Hawaiian | Yeti | Pig In a Forest | Cherry Bomb";
    public const string Pizza_AvailableToppings =
        "pepperoni | sausage | mushrooms | basil | extra cheese | extra sauce | " +
        "anchovies | pineapple | olives | arugula | Canadian bacon | Mama Lil's Peppers";

    public const string SaladIngredients = "lettuce | tomatoes | red onions | olives | peppers | parmesan | croutons";
    public const string SaladPortion = "half | whole";
    public const string SaladStyle = "Garden | Greek";

    public const string Hawaiian_Toppings = "pineapple | Canadian bacon";
    public const string Yeti_Toppings = "extra cheese | extra sauce";
    public const string PigInAForest_Toppings = "mushrooms | basil | Canadian bacon | arugula";
    public const string CherryBomb_Toppings = "pepperoni | sausage | Mama Lil's Peppers";

    public static readonly VocabCollection NamedPizzas = new VocabCollection()
    {
        {"Hawaiian", Hawaiian_Toppings },
        {"Yeti", Yeti_Toppings },
        {"Pig In a Forest", PigInAForest_Toppings},
        {"Cherry Bomb", CherryBomb_Toppings }
    };

    public static readonly VocabCollection AvailableIngredients = new VocabCollection()
    {
        {"Pizza", Pizza_AvailableToppings},
        {"Salad", SaladIngredients}
    };
}
