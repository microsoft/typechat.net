// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.TypeChat.Schema;

namespace CoffeeShop;

[JsonPolymorphic]
[JsonDerivedType(typeof(UnknownItem), typeDiscriminator: nameof(UnknownItem))]
[JsonDerivedType(typeof(EspressoDrink), typeDiscriminator: nameof(EspressoDrink))]
[JsonDerivedType(typeof(CoffeeDrink), typeDiscriminator: nameof(CoffeeDrink))]
public abstract class CartItem { }

public class Cart
{
    [JsonPropertyName("items")]
    public CartItem[] Items { get; set; }
}

[Comment("Use this type for order items that match nothing else")]
public class UnknownItem : CartItem
{
    [Comment("The text that wasn't understoodx")]
    public string Text { get; set; }
}

public abstract class LineItem : CartItem
{
    [JsonPropertyName("quantity")]
    public int Quantity { get; set; } = 1;
}

public class EspressoDrink : LineItem
{
    [Vocab(CoffeeShopVocabs.Names.EspressoDrinks)]
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("temperature")]
    public CoffeeTemperature? Temperature { get; set; }

    [JsonPropertyName("size")]
    [Comment("The default is 'Grande'")]
    public EspressoSize? Size { get; set; }

    [JsonPropertyName("option")]
    public DrinkOption? Option { get; set; }
}

public class CoffeeDrink : LineItem
{
    [Vocab(CoffeeShopVocabs.Names.CoffeeDrinks)]
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("temperature")]
    public CoffeeTemperature? Temperature { get; set; }

    [JsonPropertyName("size")]
    [Comment("The default is 'Grande'")]
    public CoffeeSize? Size { get; set; }

    [JsonPropertyName("option")]
    public DrinkOption? Options { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CoffeeSize
{
    Short,
    Tall,
    Grande,
    Venti
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CoffeeTemperature
{
    Hot,
    Extra_Hot,
    Warm,
    Iced
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EspressoSize
{
    Solo,
    Doppio,
    Triple,
    Quad
}

[JsonPolymorphic]
[JsonDerivedType(typeof(Creamer), typeDiscriminator: nameof(Creamer))]
[JsonDerivedType(typeof(Milk), typeDiscriminator: nameof(Milk))]
public abstract class DrinkOption
{
}

public class Creamer : DrinkOption
{
    [Vocab(CoffeeShopVocabs.Names.Creamers, Inline = true)]
    [JsonPropertyName("name")]
    public string Name { get; set; }
}

public class Milk : DrinkOption
{
    [Vocab(CoffeeShopVocabs.Names.Milks, Inline = true)]
    [JsonPropertyName("name")]
    public string Name { get; set; }
}

public static class CoffeeShopVocabs
{
    public static class Names
    {
        public const string CoffeeDrinks = "CoffeeDrinks";
        public const string EspressoDrinks = "EspressoDrinks";
        public const string Milks = "Milks";
        public const string Creamers = "Creamers";
    }

    public static VocabCollection All()
    {
        return new VocabCollection
        {
            CoffeeDrinks(),
            EspressoDrinks(),
            Milks(),
            Creamers()
        };
    }

    public static VocabType CoffeeDrinks()
    {
        return new VocabType(Names.CoffeeDrinks, new Vocab { "americano", "coffee" });
    }
    public static VocabType EspressoDrinks()
    {
        return new VocabType(Names.EspressoDrinks, new Vocab { "espresso", "lungo", "ristretto", "macchiato" });
    }
    public static VocabType Milks()
    {
        return new VocabType(Names.Milks, new Vocab
        {
            "whole milk",
            "two percent milk",
            "nonfat milk",
            "coconut milk",
            "soy milk",
            "almond milk",
            "oat milk"
        });
    }

    public static VocabType Creamers()
    {
        return new VocabType(Names.Creamers, new Vocab
        {
            "whole milk creamer",
            "two percent milk creamer",
            "one percent milk creamer",
            "nonfat milk creamer",
            "coconut milk creamer",
            "soy milk creamer",
            "almond milk creamer",
            "oat milk creamer",
            "half and half",
            "heavy cream"
        });
    }
}

internal static class Test
{
    public static Cart TestCart()
    {
        Cart cart = new Cart
        {
            Items = new CartItem[]
            {
                new EspressoDrink {Name = "espresso", Quantity = 1 },
                new CoffeeDrink {Name = "coffee", Size = CoffeeSize.Tall, Quantity = 2},
            }
        };
        return cart;
    }

}
