// Copyright (c) Microsoft. All rights reserved.

using System.Text.Json.Serialization;
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
    [JsonVocab(CoffeeShopVocabs.EspressoDrinks, PropertyName = "productName")]
    [JsonPropertyName("productName")]
    public string Name { get; set; }

    [JsonPropertyName("temperature")]
    public CoffeeTemperature? Temperature { get; set; }

    [JsonPropertyName("size")]
    [Comment("The default is 'Doppio'")]
    public EspressoSize? Size { get; set; } = EspressoSize.Doppio;

    [JsonPropertyName("options")]
    public DrinkOption[]? Options { get; set; }
}

public partial class CoffeeDrinks : LineItem
{
    [JsonVocab(CoffeeShopVocabs.CoffeeDrinks, PropertyName = "productName")]
    [JsonPropertyName("productName")]
    public string Name { get; set; }

    [JsonPropertyName("temperature")]
    public CoffeeTemperature? Temperature { get; set; }

    [JsonPropertyName("size")]
    [Comment("The default is 'Grande'")]
    public CoffeeSize? Size { get; set; } = CoffeeSize.Grande;

    [JsonPropertyName("options")]
    public DrinkOption[]? Options { get; set; }
}

public partial class LatteDrinks : LineItem
{
    [JsonVocab(CoffeeShopVocabs.LatteDrinks, PropertyName = "productName")]
    [JsonPropertyName("productName")]
    public string Name { get; set; }

    [JsonPropertyName("temperature")]
    public CoffeeTemperature? Temperature { get; set; }

    [JsonPropertyName("size")]
    [Comment("The default is 'Grande'")]
    public CoffeeSize? Size { get; set; } = CoffeeSize.Grande;

    [JsonPropertyName("options")]
    public DrinkOption[]? Options { get; set; }
}

public class BakeryProducts : LineItem
{
    [JsonVocab(CoffeeShopVocabs.BakeryProducts, PropertyName = "productName")]
    [JsonPropertyName("productName")]
    public string Name { get; set; }

    [JsonPropertyName("options")]
    public BakeryOptions[]? Options { get; set; }
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
public abstract partial class DrinkOption { }

[Comment("Use this type for DrinkOptions that match nothing else OR if you are NOT SURE.")]
public class UnknownDrinkOption : DrinkOption
{
    [Comment("The text that wasn't understood")]
    [JsonPropertyName("text")]
    public string Text { get; set; }
}

public class Creamers : DrinkOption
{
    [JsonVocab(CoffeeShopVocabs.Creamers)]
    [JsonPropertyName("optionName")]
    public string Name { get; set; }
}

public class Milks : DrinkOption
{
    [JsonVocab(CoffeeShopVocabs.Milks)]
    [JsonPropertyName("optionName")]
    public string Name { get; set; }
}

public class Caffeines : DrinkOption
{
    [JsonVocab(CoffeeShopVocabs.Caffeines)]
    [JsonPropertyName("optionName")]
    public string Name { get; set; }
}

public class Sweetners : DrinkOption
{
    [JsonVocab(CoffeeShopVocabs.Sweetners)]
    [JsonPropertyName("optionName")]
    public string Name { get; set; }

    [JsonPropertyName("optionQuantity")]
    public OptionQuantity? Quantity { get; set; }
}

public class Syrups : DrinkOption
{
    [JsonVocab(CoffeeShopVocabs.Syrups)]
    [JsonPropertyName("optionName")]
    public string Name { get; set; }

    [JsonPropertyName("optionQuantity")]
    public OptionQuantity? Quantity { get; set; }
}

public class Toppings : DrinkOption
{
    [JsonVocab(CoffeeShopVocabs.Toppings)]
    [JsonPropertyName("optionName")]
    public string Name { get; set; }

    [JsonPropertyName("optionQuantity")]
    public OptionQuantity? Quantity { get; set; }
}

public class LattePreparations : DrinkOption
{
    [JsonVocab(CoffeeShopVocabs.LattePreparations)]
    [JsonPropertyName("optionName")]
    public string Name { get; set; }
}

[JsonDerivedType(typeof(BakeryToppings), typeDiscriminator: nameof(BakeryToppings))]
[JsonDerivedType(typeof(BakeryPreparations), typeDiscriminator: nameof(BakeryPreparations))]
public abstract class BakeryOptions { }

public class BakeryToppings : BakeryOptions
{
    [JsonVocab(CoffeeShopVocabs.BakeryToppings)]
    [JsonPropertyName("name")]
    public string Name { get; set; }
}

public class BakeryPreparations : BakeryOptions
{
    [JsonVocab(CoffeeShopVocabs.BakeryPreparations)]
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
    [JsonVocab(CoffeeShopVocabs.OptionQuantity)]
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
/// You can supply them to the schema generator using the <see cref="IVocabCollection"/> interface.
/// A real coffee shop will do just that - so that product lines can change, different users can be offered
/// different options, etc
/// </summary>
public static class CoffeeShopVocabs
{
    public const string EspressoDrinks = "espresso | lungo | ristretto | macchiato";

    public const string CoffeeDrinks = "americano | coffee";

    public const string LatteDrinks = "cappuccino | flat white | latte | latte macchiato| mocha | chai latte";

    public const string Creamers =
        "whole milk creamer | two percent milk creamer | one percent milk creamer | " +
        "nonfat milk creamer | coconut milk creamer | soy milk creamer | " +
        "almond milk creamer | oat milk creamer | half and half | heavy cream";

    public const string Milks =
        "whole milk | two percent milk | nonfat milk | coconut milk | " +
        "soy milk | almond milk | oat milk";

    public const string Caffeines = "regular | two thirds caf | half caf | one third caf | decaf";
    public const string Toppings = "cinnamon | foam | ice | nutmeg | whipped cream | water";
    public const string Sweetners = "equal | honey | splenda | sugar | sugar in the raw | sweet n low";
    public const string Syrups =
        "almond syrup | buttered rum syrup | caramel syrup | cinnamon syrup | " +
        "hazelnut syrup | orange syrup | peppermint syrup | raspberry syrup | " +
        "toffee syrup | vanilla syrup";

    public const string LattePreparations = "for here cup | lid | with room | to go | dry | wet";

    public const string BakeryProducts = "apple bran muffin | blueberry muffin | lemon poppyseed muffin | bagel";
    public const string BakeryToppings = "butter | strawberry jam | cream cheese";
    public const string BakeryPreparations = "warmed | cut in half";
    public const string OptionQuantity = "no | light | regular | extra";
}
