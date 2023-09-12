// Copyright (c) Microsoft. All rights reserved.

using System.ComponentModel.DataAnnotations;
using System.Text.Json;

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

public class Creamer
{
    [JsonVocab(Name = TestVocabs.Names.Creamers)]
    public string Name { get; set; }
}

public class Milk
{
    [JsonVocab(Name = TestVocabs.Names.Milks, Inline = true)]
    [JsonPropertyName("name")]
    public string Name { get; set; }
}

public class DessertOrder
{
    public DessertOrder() { }
    public DessertOrder(string name, int quantity)
    {
        Name = name;
        Quantity = quantity;
    }

    [JsonVocab(Name = TestVocabs.Names.Desserts)]
    [JsonPropertyName("dessert")]
    public string Name { get; set; }
    [JsonPropertyName("quantity")]
    public int Quantity { get; set; } = 1;

    public static implicit operator DessertOrder(string name)
    {
        return new DessertOrder(name, 1);
    }
}

public class FruitOrder
{
    [JsonVocab(Name = TestVocabs.Names.Fruits)]
    [JsonPropertyName("fruit")]
    public string Name { get; set; }
    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }
}

public class UnknownItem
{
    [Comment("The text that wasn't understood")]
    [JsonPropertyName("text")]
    public string Text { get; set; }
}

public class Order
{
    [JsonPropertyName("coffee")]
    public CoffeeOrder[] Coffees { get; set; }
    [JsonPropertyName("desserts")]
    public DessertOrder[] Desserts { get; set; }
    [JsonPropertyName("fruits")]
    public FruitOrder[] Fruits { get; set; }
    [JsonPropertyName("unknown")]
    public UnknownItem[] Unknown { get; set; }
}

public class SentimentResponse
{
    [JsonPropertyName("sentiment")]
    public string Sentiment { get; set; }
}

public class NullableTestObj
{
    public CoffeeSize Required;
    public CoffeeSize? Optional;
    public string Text;

    [JsonPropertyName("optionalText")]
    public string? OptionalText { get; set; }

    public string? OptionalTextField { get; set; }

    public int Amt;
    public int? OptionalAmt;
}

public class WrapperNullableObj
{
    public NullableTestObj? Test { get; set; }

    [JsonVocab(Name = TestVocabs.Names.Milks, Inline = true)]
    [JsonPropertyName("milk")]
    public string? OptionalMilk { get; set; }
}

public class ConverterTestObj
{
    [JsonVocab("Whole | Two Percent | Almond | Soy")]
    public string Milk { get; set; }
}

public class HardcodedVocabObj
{
    public const string VocabName = "Local";

    [JsonVocab("One | Two | Three | Four", Name = VocabName)]
    public string Value { get; set; }
}

public class JsonFunc
{
    public string Name { get; set; }
}

public class JsonExpr
{
    public JsonFunc Func;
    public JsonElement Value;
}

public static class TestVocabs
{
    public static class Names
    {
        public const string Desserts = "Desserts";
        public const string Fruits = "Fruits";
        public const string Milks = "Milks";
        public const string Creamers = "Creamers";
    }

    public static VocabCollection All()
    {
        return new VocabCollection
        {
            Desserts(),
            Fruits(),
            Milks(),
            Creamers()
        };
    }

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
        return new VocabType(Names.Desserts, vocab);
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

        return new VocabType(Names.Fruits, vocab);
    }

    public static VocabType Milks()
    {
        Vocab vocab = new Vocab
        {
            "whole milk",
            "two percent milk",
            "nonfat milk",
            "coconut milk",
            "soy milk",
            "almond milk",
            "oat milk"
        };
        return new VocabType(Names.Milks, vocab);
    }

    public static VocabType Creamers()
    {
        Vocab vocab = new Vocab
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
        };
        return new VocabType(Names.Creamers, vocab);
    }
}

public class Person
{
    [ValidateObject]
    public Name Name { get; set; }

    [Range(0, 105, ErrorMessage = "Person.Age must be between 0 and 105")]
    public int Age { get; set; }

    public Location Location { get; set; }

    public bool HasSameName(Person other)
    {
        return (this.Name.CompareTo(other.Name) == 0);
    }

    public void ChangeCase(bool upper)
    {
        Name.ChangeCase(upper);
        Location.ChangeCase(upper);
    }
}

public class Name : IComparable<Name>
{
    [StringLength(32, ErrorMessage = "FirstName must be <= 32 characters")]
    public string FirstName { get; set; }

    [StringLength(32)]
    public string LastName { get; set; }

    public void ChangeCase(bool upper)
    {
        if (upper)
        {
            FirstName = FirstName.ToUpper();
            LastName = LastName.ToUpper();
        }
        else
        {
            FirstName = FirstName.ToLower();
            LastName = LastName.ToLower();
        }
    }

    public int CompareTo(Name other)
    {
        int cmp = string.Compare(FirstName, other.FirstName, StringComparison.OrdinalIgnoreCase);
        if (cmp == 0)
        {
            cmp = string.Compare(LastName, other.LastName, StringComparison.OrdinalIgnoreCase);
        }
        return cmp;
    }
}

public class Location
{
    public string City { get; set; }
    public string State { get; set; }
    public string Country { get; set; }

    public void ChangeCase(bool upper)
    {
        if (upper)
        {
            City = City.ToUpper();
            State = State.ToUpper();
            Country = Country.ToUpper();
        }
        else
        {
            City = City.ToLower();
            State = State.ToLower();
            Country = Country.ToLower();
        }
    }
}

[JsonPolymorphic]
[JsonDerivedType(typeof(Rectangle), typeDiscriminator: nameof(Rectangle))]
[JsonDerivedType(typeof(Circle), typeDiscriminator: nameof(Circle))]
public class Shape
{
    public string Id;
}

public class Rectangle : Shape
{
    public double TopX;
    public double TopY;
    public double Height;
    public double Width;
}

public class Circle : Shape
{
    public double CenterX;
    public double CenterY;
    public double Radius;
}

public class Canvas
{
    public Shape[] Shapes { get; set; }

    public T? GetShape<T>(int objNumber = 0)
        where T : Shape
    {
        int curNumber = 0;
        foreach (var shape in Shapes)
        {
            if (shape.GetType() == typeof(T))
            {
                if (curNumber == objNumber)
                {
                    return (T) shape;
                }
            }
        }
        return null;
    }
}
