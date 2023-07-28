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

[Comment("Always use ")]
public class Cart : IConstraintValidatable
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

    public void ValidateConstraints(ConstraintCheckContext context) => Items.ValidateConstraints(context);
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
    [Vocab(CoffeeShopVocabs.Names.EspressoDrinks)]
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

public class CoffeeDrink : LineItem, IConstraintValidatable
{
    [Vocab(CoffeeShopVocabs.Names.CoffeeDrinks)]
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

    public void ValidateConstraints(ConstraintCheckContext context) => Options.ValidateConstraints(context);
}

public class LatteDrink : LineItem, IConstraintValidatable
{
    [Vocab(CoffeeShopVocabs.Names.LatteDrinks)]
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

    public void ValidateConstraints(ConstraintCheckContext context)
    {
        context.CheckVocabEntry("productName", CoffeeShopVocabs.Names.LatteDrinks, Name);
    }
}

public class BakeryItem : LineItem
{
    [Vocab(CoffeeShopVocabs.Names.BakeryProducts)]
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
    [Vocab(CoffeeShopVocabs.Names.Creamers)]
    [JsonPropertyName("optionName")]
    public string Name { get; set; }
}

public class Milk : DrinkOption
{
    [Vocab(CoffeeShopVocabs.Names.Milks)]
    [JsonPropertyName("optionName")]
    public string Name { get; set; }
}

public class Caffeine : DrinkOption
{
    [Vocab(CoffeeShopVocabs.Names.Caffeines)]
    [JsonPropertyName("optionName")]
    public string Name { get; set; }
}

public class Sweetner : DrinkOption
{
    [Vocab(CoffeeShopVocabs.Names.Sweetners)]
    [JsonPropertyName("optionName")]
    public string Name { get; set; }

    [JsonPropertyName("optionQuantity")]
    public OptionQuantity? Quantity { get; set; }
}

public class Syrup : DrinkOption, IConstraintValidatable
{
    [Vocab(CoffeeShopVocabs.Names.Syrups)]
    [JsonPropertyName("optionName")]
    public string Name { get; set; }

    [JsonPropertyName("optionQuantity")]
    public OptionQuantity? Quantity { get; set; }

    public virtual void ValidateConstraints(ConstraintCheckContext context)
    {
        context.CheckVocabEntry("optionName", CoffeeShopVocabs.Names.Syrups, Name);
    }
}

public class Topping : DrinkOption
{
    [Vocab(CoffeeShopVocabs.Names.Toppings)]
    [JsonPropertyName("optionName")]
    public string Name { get; set; }

    [JsonPropertyName("optionQuantity")]
    public OptionQuantity? Quantity { get; set; }
}

public class LattePreparation : DrinkOption
{
    [Vocab(CoffeeShopVocabs.Names.LattePreparations)]
    [JsonPropertyName("optionName")]
    public string Name { get; set; }
}

[JsonDerivedType(typeof(BakeryTopping), typeDiscriminator: nameof(BakeryTopping))]
[JsonDerivedType(typeof(BakeryPreparation), typeDiscriminator: nameof(BakeryPreparation))]
public abstract class BakeryOption { }

public class BakeryTopping : BakeryOption
{
    [Vocab(CoffeeShopVocabs.Names.BakeryToppings)]
    [JsonPropertyName("name")]
    public string Name { get; set; }
}

public class BakeryPreparation : BakeryOption
{
    [Vocab(CoffeeShopVocabs.Names.BakeryPreparations)]
    [JsonPropertyName("name")]
    public string Name { get; set; }
}

[JsonPolymorphic]
[JsonDerivedType(typeof(StringOptionQuantity), typeDiscriminator: nameof(StringOptionQuantity))]
[JsonDerivedType(typeof(NumberOptionQuantity), typeDiscriminator: nameof(NumberOptionQuantity))]
public abstract class OptionQuantity
{
}

public class StringOptionQuantity : OptionQuantity
{
    [Vocab(CoffeeShopVocabs.Names.OptionQuantity)]
    [JsonPropertyName("amount")]
    public string Amount { get; set; }
}

public class NumberOptionQuantity : OptionQuantity
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
    public static class Names
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

    public static VocabCollection All()
    {
        return new VocabCollection
        {
            CoffeeDrinks(),
            EspressoDrinks(),
            LatteDrinks(),

            Milks(),
            Creamers(),
            Caffeines(),
            Sweetners(),
            Syrups(),
            Toppings(),
            LattePreparations(),

            BakeryProducts(),
            BakeryToppings(),
            BakeryPreparations(),

            OptionQuantities()
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

    public static VocabType LatteDrinks()
    {
        return new VocabType(Names.LatteDrinks, new Vocab
        {
            "cappuccino",
            "flat white",
            "latte",
            "latte macchiato",
            "mocha",
            "chai latte"
        });
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

    public static VocabType Caffeines()
    {
        return new VocabType(Names.Caffeines, new Vocab
        {
            "regular",
            "two thirds caf",
            "half caf",
            "one third caf",
            "decaf"
        });
    }

    public static VocabType Toppings()
    {
        return new VocabType(Names.Toppings, new Vocab
        {
            "cinnamon",
            "foam",
            "ice",
            "nutmeg",
            "whipped cream",
            "water"
        });
    }

    public static VocabType Sweetners()
    {
        return new VocabType(Names.Sweetners, new Vocab
        {
            "equal",
            "honey",
            "splenda",
            "sugar",
            "sugar in the raw",
            "sweet n low"
        });
    }

    public static VocabType Syrups()
    {
        return new VocabType(Names.Syrups, new Vocab
        {
            "almond syrup",
            "buttered rum syrup",
            "caramel syrup",
            "cinnamon syrup",
            "hazelnut syrup",
            "orange syrup",
            "peppermint syrup",
            "raspberry syrup",
            "toffee syrup",
            "vanilla syrup"
        });
    }

    public static VocabType LattePreparations()
    {
        return new VocabType(Names.LattePreparations, new Vocab
        {
            "for here cup",
            "lid",
            "with room",
            "to go",
            "dry",
            "wet"
        });
    }

    public static VocabType BakeryProducts()
    {
        return new VocabType(Names.BakeryProducts, new Vocab { "apple bran muffin", "blueberry muffin", "lemon poppyseed muffin", "bagel" });
    }

    public static VocabType BakeryToppings()
    {
        return new VocabType(Names.BakeryToppings, new Vocab { "butter", "strawberry jam", "cream cheese" });
    }

    public static VocabType BakeryPreparations()
    {
        return new VocabType(Names.BakeryPreparations, new Vocab { "warmed", "cut in half" });
    }

    public static VocabType OptionQuantities()
    {
        return new VocabType(Names.OptionQuantity, new Vocab { "no", "light", "regular", "extra" });
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
