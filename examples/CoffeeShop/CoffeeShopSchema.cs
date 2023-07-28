// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.TypeChat;
using Microsoft.TypeChat.Schema;

namespace CoffeeShop;

/*
    THIS IS A *CODE FIRST* SCHEMA FOR A COFFEE SHOP
    The serialization format is System.Text.Json
    THIS IS AUTO CONVERTED TO Typescript schema at runtime.

    Do not rename properties or refactor without testing.
    The model can be VERY sensitive to property names.
    E.g. changing 'productName' to 'name' can lead to the model being less stringent about what it places in that field. 
 */

public class Cart
{
    [JsonPropertyName("items")]
    public CartItem[] Items { get; set; }

    public virtual void GetUnknown(StringBuilder sb)
    {
        if (Items != null)
        {
            foreach (CartItem item in Items)
            {
                item.GetUnknown(sb);
            }
        }
    }
}

[JsonPolymorphic]
[JsonDerivedType(typeof(UnknownItem), typeDiscriminator: nameof(UnknownItem))]
[JsonDerivedType(typeof(EspressoDrink), typeDiscriminator: nameof(EspressoDrink))]
[JsonDerivedType(typeof(CoffeeDrink), typeDiscriminator: nameof(CoffeeDrink))]
[JsonDerivedType(typeof(LatteDrink), typeDiscriminator: nameof(LatteDrink))]
[JsonDerivedType(typeof(BakeryItem), typeDiscriminator: nameof(BakeryItem))]
public abstract class CartItem
{
    public virtual void GetUnknown(StringBuilder sb) { return; }
}

[Comment("Use this type for products with names that match NO listed PRODUCT NAME")]
public class UnknownItem : CartItem
{
    [Comment("The text that wasn't understood")]
    [JsonPropertyName("text")]
    public string Text { get; set; }

    public override void GetUnknown(StringBuilder sb)
    {
        sb.AppendLine(Text);
    }
}

[Comment("ONLY USE NAMES that productName is assignable to")]
public abstract class LineItem : CartItem
{
    [JsonPropertyName("quantity")]
    public int Quantity { get; set; } = 1;
}

public class EspressoDrink : LineItem
{
    [Vocab(VocabNames.EspressoDrinks, "espresso | lungo | ristretto | macchiato")]
    [JsonPropertyName("productName")]
    public string Name { get; set; }

    [JsonPropertyName("temperature")]
    public CoffeeTemperature? Temperature { get; set; }

    [JsonPropertyName("size")]
    [Comment("The default is 'Doppio'")]
    public EspressoSize? Size { get; set; } = EspressoSize.Doppio;

    [JsonPropertyName("options")]
    public DrinkOption[]? Options { get; set; }

    public override void GetUnknown(StringBuilder sb) => Options.GetUnknown(sb);
}

public class CoffeeDrink : LineItem
{    
    [Vocab(VocabNames.CoffeeDrinks, "americano | coffee")]
    [JsonPropertyName("productName")]
    public string Name { get; set; }

    [JsonPropertyName("temperature")]
    public CoffeeTemperature? Temperature { get; set; }

    [JsonPropertyName("size")]
    [Comment("The default is 'Grande'")]
    public CoffeeSize? Size { get; set; } = CoffeeSize.Grande;

    [JsonPropertyName("options")]
    public DrinkOption[]? Options { get; set; }

    public override void GetUnknown(StringBuilder sb) => Options.GetUnknown(sb);
}

public class LatteDrink : LineItem
{
    [Vocab(
        VocabNames.LatteDrinks,
        "cappuccino | flat white | latte | latte macchiato | mocha | chai latte"
    )]
    [JsonPropertyName("productName")]
    public string Name { get; set; }

    [JsonPropertyName("temperature")]
    public CoffeeTemperature? Temperature { get; set; }

    [JsonPropertyName("size")]
    [Comment("The default is 'Grande'")]
    public CoffeeSize? Size { get; set; } = CoffeeSize.Grande;

    [JsonPropertyName("options")]
    public DrinkOption[]? Options { get; set; }

    public override void GetUnknown(StringBuilder sb) => Options.GetUnknown(sb);
}

public class BakeryItem : LineItem
{
    [Vocab(
        VocabNames.BakeryProducts,
        "apple bran muffin | blueberry muffin | lemon poppyseed muffin | bagel"
    )]
    [JsonPropertyName("productName")]
    public string Name { get; set; }

    [JsonPropertyName("options")]
    public BakeryOption[]? Options { get; set; }
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
[JsonDerivedType(typeof(UnknownDrinkOption), typeDiscriminator: nameof(UnknownDrinkOption))]
[JsonDerivedType(typeof(Creamer), typeDiscriminator: nameof(Creamer))]
[JsonDerivedType(typeof(Milk), typeDiscriminator: nameof(Milk))]
[JsonDerivedType(typeof(Caffeine), typeDiscriminator: nameof(Caffeine))]
[JsonDerivedType(typeof(Sweetner), typeDiscriminator: nameof(Sweetner))]
[JsonDerivedType(typeof(Syrup), typeDiscriminator: nameof(Syrup))]
[JsonDerivedType(typeof(Topping), typeDiscriminator: nameof(Topping))]
[JsonDerivedType(typeof(LattePreparation), typeDiscriminator: nameof(LattePreparation))]
public abstract class DrinkOption
{
    public virtual void GetUnknown(StringBuilder sb)
    {
        return;
    }
}

[Comment("Use this type for DrinkOptions that match nothing else OR if you are NOT SURE. ")]
public class UnknownDrinkOption : DrinkOption
{
    [Comment("The text that wasn't understood")]
    [JsonPropertyName("text")]
    public string Text { get; set; }

    public override void GetUnknown(StringBuilder sb)
    {
        sb.AppendLine(Text);
    }
}

public class Creamer : DrinkOption
{
    [Vocab(
        VocabNames.Creamers,
        "whole milk creamer | two percent milk creamer | one percent milk creamer | " +
        "nonfat milk creamer | coconut milk creamer | soy milk creamer | " +
        "almond milk creamer | oat milk creamer | half and half | heavy cream"
    )]
    [JsonPropertyName("optionName")]
    public string Name { get; set; }
}

public class Milk : DrinkOption
{
    [Vocab(
        VocabNames.Milks,
        "whole milk | two percent milk | nonfat milk | coconut milk | " +
        "soy milk | almond milk | oat milk"
    )]
    [JsonPropertyName("optionName")]
    public string Name { get; set; }
}

public class Caffeine : DrinkOption
{
    [Vocab(
        VocabNames.Caffeines,
        "cinnamon | foam | ice | nutmeg | whipped cream | water"
    )]
    [JsonPropertyName("optionName")]
    public string Name { get; set; }
}

public class Sweetner : DrinkOption
{
    [Vocab(
        VocabNames.Sweetners,
        "equal | honey | splenda | sugar | sugar in the raw | sweet n low"
    )]
    [JsonPropertyName("optionName")]
    public string Name { get; set; }

    [JsonPropertyName("optionQuantity")]
    public OptionQuantity? Quantity { get; set; }
}

public class Syrup : DrinkOption
{
    [Vocab(
        VocabNames.Syrups,
        "almond syrup | buttered rum syrup | caramel syrup | cinnamon syrup | " +
        "hazelnut syrup | orange syrup | peppermint syrup | raspberry syrup | " +
        "toffee syrup | vanilla syrup"
    )]
    [JsonPropertyName("optionName")]
    public string Name { get; set; }

    [JsonPropertyName("optionQuantity")]
    public OptionQuantity? Quantity { get; set; }
}

public class Topping : DrinkOption
{
    [Vocab(
        VocabNames.Toppings,
        "cinnamon | foam | ice | nutmeg | whipped cream | water"
    )]
    [JsonPropertyName("optionName")]
    public string Name { get; set; }

    [JsonPropertyName("optionQuantity")]
    public OptionQuantity? Quantity { get; set; }
}

public class LattePreparation : DrinkOption
{
    [Vocab(
        VocabNames.LattePreparations,
        "for here cup | lid | with room | to go | dry | wet"
    )]
    [JsonPropertyName("optionName")]
    public string Name { get; set; }
}

[JsonDerivedType(typeof(BakeryTopping), typeDiscriminator: nameof(BakeryTopping))]
[JsonDerivedType(typeof(BakeryPreparation), typeDiscriminator: nameof(BakeryPreparation))]
public abstract class BakeryOption { }

public class BakeryTopping : BakeryOption
{
    [Vocab(VocabNames.BakeryToppings, "butter | strawberry jam | cream cheese")]
    [JsonPropertyName("name")]
    public string Name { get; set; }
}

public class BakeryPreparation : BakeryOption
{
    [Vocab(VocabNames.BakeryPreparations, "warmed | cut in half")]
    [JsonPropertyName("name")]
    public string Name { get; set; }
}

[JsonPolymorphic]
[JsonDerivedType(typeof(StringQuantity), typeDiscriminator: nameof(StringQuantity))]
[JsonDerivedType(typeof(NumberQuantity), typeDiscriminator: nameof(NumberQuantity))]
public abstract class OptionQuantity
{
}

public class StringQuantity : OptionQuantity
{
    [Vocab(VocabNames.OptionQuantity, "no | light | regular | extra")]
    [JsonPropertyName("amount")]
    public string Amount { get; set; }
}

public class NumberQuantity : OptionQuantity
{
    [JsonPropertyName("amount")]
    public int Amount { get; set; }
}

/// <summary>
/// Coffeshop vocabulary is hardcoded here for simplicity
/// But you can always load these vocabularies from a store or database dynamically.
/// You can supply them to the schema generator using the IVocabCollection interface.
/// A real coffee shop will do just that - so that product lines can change, different users can be offered
/// different options, etc
/// </summary>
public static class VocabNames
{
    public const string CoffeeDrinks = "CoffeeDrinks";
    public const string EspressoDrinks = "EspressoDrinks";
    public const string LatteDrinks = "LatteDrinks";
    public const string Creamers = "Creamers";
    public const string Milks = "Milks";
    public const string Caffeines = "Caffeines";
    public const string Toppings = "Toppings";
    public const string Sweetners = "Sweetners";
    public const string Syrups = "Syrups";
    public const string LattePreparations = "LattePreparations";

    public const string BakeryProducts = "BakeryProducts";
    public const string BakeryToppings = "BakeryToppings";
    public const string BakeryPreparations = "BakeryPreparations";
    public const string OptionQuantity = "OptionQuantity";
}

internal static class CartEx
{
    public static void GetUnknown(this DrinkOption[] options, StringBuilder sb)
    {
        if (options != null)
        {
            foreach (var option in options)
            {
                option.GetUnknown(sb);
            }
        }
    }

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
