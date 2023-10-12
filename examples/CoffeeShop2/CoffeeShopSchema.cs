// Copyright (c) Microsoft. All rights reserved.

using System.Text.Json.Serialization;
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

public partial class Cart
{
    [JsonPropertyName("items")]
    public CartItem[] Items { get; set; }
}

[JsonPolymorphic]
[JsonDerivedType(typeof(UnknownItem), typeDiscriminator: nameof(UnknownItem))]
[JsonDerivedType(typeof(EspressoDrinks), typeDiscriminator: nameof(EspressoDrinks))]
[JsonDerivedType(typeof(CoffeeDrinks), typeDiscriminator: nameof(CoffeeDrinks))]
[JsonDerivedType(typeof(LatteDrinks), typeDiscriminator: nameof(LatteDrinks))]
[JsonDerivedType(typeof(BakeryProducts), typeDiscriminator: nameof(BakeryProducts))]
public abstract partial class CartItem
{
}

[Comment("Use this type for Cart Items that match nothing else")]
public partial class UnknownItem : CartItem
{
    [Comment("The text that wasn't understood")]
    [JsonPropertyName("text")]
    public string Text { get; set; }
}

public abstract class LineItem : CartItem
{
    [JsonPropertyName("quantity")]
    public int Quantity { get; set; } = 1;
}

public partial class EspressoDrinks : LineItem
{
    [JsonVocab(Name = CoffeeShopVocabs.EspressoDrinks, PropertyName = "productName")]
    [JsonPropertyName("productName")]
    public string Name { get; set; }

    [JsonPropertyName("temperature")]
    public CoffeeTemperature? Temperature { get; set; }

    [JsonPropertyName("size")]
    [Comment("The default is 'Doppio'")]
    public EspressoSize? Size { get; set; }

    [JsonPropertyName("options")]
    public DrinkOption[]? Options { get; set; }
}

public partial class CoffeeDrinks : LineItem
{
    [JsonVocab(Name = CoffeeShopVocabs.CoffeeDrinks, PropertyName = "productName")]
    [JsonPropertyName("productName")]
    public string Name { get; set; }

    [JsonPropertyName("temperature")]
    public CoffeeTemperature? Temperature { get; set; }

    [JsonPropertyName("size")]
    [Comment("The default is 'Grande'")]
    public CoffeeSize? Size { get; set; }

    [JsonPropertyName("options")]
    public DrinkOption[]? Options { get; set; }
}

public partial class LatteDrinks : LineItem
{
    [JsonVocab(Name = CoffeeShopVocabs.LatteDrinks, PropertyName = "productName")]
    [JsonPropertyName("productName")]
    public string Name { get; set; }

    [JsonPropertyName("temperature")]
    public CoffeeTemperature? Temperature { get; set; }

    [JsonPropertyName("size")]
    [Comment("The default is 'Grande'")]
    public CoffeeSize? Size { get; set; }

    [JsonPropertyName("options")]
    public DrinkOption[]? Options { get; set; }
}

public class BakeryProducts : LineItem
{
    [JsonVocab(Name = CoffeeShopVocabs.BakeryProducts, PropertyName = "productName")]
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
[JsonDerivedType(typeof(Creamers), typeDiscriminator: nameof(Creamers))]
[JsonDerivedType(typeof(Milks), typeDiscriminator: nameof(Milks))]
[JsonDerivedType(typeof(Caffeines), typeDiscriminator: nameof(Caffeines))]
[JsonDerivedType(typeof(Sweetners), typeDiscriminator: nameof(Sweetners))]
[JsonDerivedType(typeof(Syrups), typeDiscriminator: nameof(Syrups))]
[JsonDerivedType(typeof(Toppings), typeDiscriminator: nameof(Toppings))]
[JsonDerivedType(typeof(LattePreparations), typeDiscriminator: nameof(LattePreparations))]
[Comment("ABSTRACT interface")]
public abstract partial class DrinkOption
{
}

[Comment("Use this type for DrinkOptions that match nothing else OR if you are NOT SURE. ")]
public partial class UnknownDrinkOption : DrinkOption
{
    [Comment("The text that wasn't understood")]
    [JsonPropertyName("text")]
    public string Text { get; set; }
}

public class Creamers : DrinkOption
{
    [JsonVocab(Name = CoffeeShopVocabs.Creamers)]
    [JsonPropertyName("optionName")]
    public string Name { get; set; }
}

public class Milks : DrinkOption
{
    [JsonVocab(Name = CoffeeShopVocabs.Milks)]
    [JsonPropertyName("optionName")]
    public string Name { get; set; }
}

public class Caffeines : DrinkOption
{
    [JsonVocab(Name = CoffeeShopVocabs.Caffeines)]
    [JsonPropertyName("optionName")]
    public string Name { get; set; }
}

public class Sweetners : DrinkOption
{
    [JsonVocab(Name = CoffeeShopVocabs.Sweetners)]
    [JsonPropertyName("optionName")]
    public string Name { get; set; }

    [JsonPropertyName("optionQuantity")]
    public OptionQuantity? Quantity { get; set; }
}

public class Syrups : DrinkOption
{
    [JsonVocab(Name = CoffeeShopVocabs.Syrups)]
    [JsonPropertyName("optionName")]
    public string Name { get; set; }

    [JsonPropertyName("optionQuantity")]
    public OptionQuantity? Quantity { get; set; }
}

public class Toppings : DrinkOption
{
    [JsonVocab(Name = CoffeeShopVocabs.Toppings)]
    [JsonPropertyName("optionName")]
    public string Name { get; set; }

    [JsonPropertyName("optionQuantity")]
    public OptionQuantity? Quantity { get; set; }
}

public class LattePreparations : DrinkOption
{
    [JsonVocab(Name = CoffeeShopVocabs.LattePreparations)]
    [JsonPropertyName("optionName")]
    public string Name { get; set; }
}

[JsonDerivedType(typeof(BakeryToppings), typeDiscriminator: nameof(BakeryToppings))]
[JsonDerivedType(typeof(BakeryPreparations), typeDiscriminator: nameof(BakeryPreparations))]
public abstract class BakeryOption { }

public class BakeryToppings : BakeryOption
{
    [JsonVocab(Name = CoffeeShopVocabs.BakeryToppings)]
    [JsonPropertyName("name")]
    public string Name { get; set; }
}

public class BakeryPreparations : BakeryOption
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
