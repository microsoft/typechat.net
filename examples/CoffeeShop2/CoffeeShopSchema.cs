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
    It includes CONSTRAINTS VALIDATION

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

public abstract class LineItem : CartItem
{
    [JsonPropertyName("quantity")]
    public int Quantity { get; set; } = 1;
}

public class EspressoDrink : LineItem
{
    [JsonVocab(Name = CoffeeShopVocabs.EspressoDrinks)]
    [JsonPropertyName("productName")]
    public string Name { get; set; }

    [JsonPropertyName("temperature")]
    public CoffeeTemperature? Temperature { get; set; }

    [JsonPropertyName("size")]
    [Comment("The default is 'Doppio'")]
    public EspressoSize? Size { get; set; }

    [JsonPropertyName("options")]
    public DrinkOption[]? Options { get; set; }

    public override void GetUnknown(StringBuilder sb) => Options.GetUnknown(sb);
}

public class CoffeeDrink : LineItem
{
    [JsonVocab(Name = CoffeeShopVocabs.CoffeeDrinks)]
    [JsonPropertyName("productName")]
    public string Name { get; set; }

    [JsonPropertyName("temperature")]
    public CoffeeTemperature? Temperature { get; set; }

    [JsonPropertyName("size")]
    [Comment("The default is 'Grande'")]
    public CoffeeSize? Size { get; set; }

    [JsonPropertyName("options")]
    public DrinkOption[]? Options { get; set; }

    public override void GetUnknown(StringBuilder sb) => Options.GetUnknown(sb);
}

public class LatteDrink : LineItem
{
    [JsonVocab(Name = CoffeeShopVocabs.LatteDrinks)]
    [JsonPropertyName("productName")]
    public string Name { get; set; }

    [JsonPropertyName("temperature")]
    public CoffeeTemperature? Temperature { get; set; }

    [JsonPropertyName("size")]
    [Comment("The default is 'Grande'")]
    public CoffeeSize? Size { get; set; }

    [JsonPropertyName("options")]
    public DrinkOption[]? Options { get; set; }

    public override void GetUnknown(StringBuilder sb) => Options.GetUnknown(sb);
}

public class BakeryItem : LineItem
{
    [JsonVocab(Name = CoffeeShopVocabs.BakeryProducts)]
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
    public virtual void GetUnknown(StringBuilder sb) { return; }
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
    [JsonVocab(Name = CoffeeShopVocabs.Creamers)]
    [JsonPropertyName("optionName")]
    public string Name { get; set; }
}

public class Milk : DrinkOption
{
    [JsonVocab(Name = CoffeeShopVocabs.Milks)]
    [JsonPropertyName("optionName")]
    public string Name { get; set; }
}

public class Caffeine : DrinkOption
{
    [JsonVocab(Name = CoffeeShopVocabs.Caffeines)]
    [JsonPropertyName("optionName")]
    public string Name { get; set; }
}

public class Sweetner : DrinkOption
{
    [JsonVocab(Name = CoffeeShopVocabs.Sweetners)]
    [JsonPropertyName("optionName")]
    public string Name { get; set; }

    [JsonPropertyName("optionQuantity")]
    public OptionQuantity? Quantity { get; set; }
}

public class Syrup : DrinkOption
{
    [JsonVocab(Name = CoffeeShopVocabs.Syrups)]
    [JsonPropertyName("optionName")]
    public string Name { get; set; }

    [JsonPropertyName("optionQuantity")]
    public OptionQuantity? Quantity { get; set; }
}

public class Topping : DrinkOption
{
    [JsonVocab(Name = CoffeeShopVocabs.Toppings)]
    [JsonPropertyName("optionName")]
    public string Name { get; set; }

    [JsonPropertyName("optionQuantity")]
    public OptionQuantity? Quantity { get; set; }
}

public class LattePreparation : DrinkOption
{
    [JsonVocab(Name = CoffeeShopVocabs.LattePreparations)]
    [JsonPropertyName("optionName")]
    public string Name { get; set; }
}

[JsonDerivedType(typeof(BakeryTopping), typeDiscriminator: nameof(BakeryTopping))]
[JsonDerivedType(typeof(BakeryPreparation), typeDiscriminator: nameof(BakeryPreparation))]
public abstract class BakeryOption { }

public class BakeryTopping : BakeryOption
{
    [JsonVocab(Name = CoffeeShopVocabs.BakeryToppings)]
    [JsonPropertyName("name")]
    public string Name { get; set; }
}

public class BakeryPreparation : BakeryOption
{
    [JsonVocab(Name = CoffeeShopVocabs.BakeryPreparations)]
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
    [JsonVocab(Name = CoffeeShopVocabs.OptionQuantity)]
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
public static class CoffeeShopVocabs
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

    public static VocabCollection Load()
    {
        return VocabFile.Load("CoffeeShopVocab.json");
    }

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
