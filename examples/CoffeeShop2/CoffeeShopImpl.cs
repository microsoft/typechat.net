﻿// Copyright (c) Microsoft. All rights reserved.
using System.Text;

namespace CoffeeShop;

/**
 * We use partial classes to deliberately keep SCHEMA separate from implementation
 * CoffeeShopSchema.cs defines the shape of the data
 * CoffeeShopImpl.cs has the code. 
 **/

public partial class Cart
{
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

public abstract partial class CartItem
{
    public virtual void GetUnknown(StringBuilder sb) { return; }
}

public partial class UnknownItem : CartItem
{
    public override void GetUnknown(StringBuilder sb)
    {
        sb.AppendLine(Text);
    }
}

public partial class EspressoDrink : LineItem
{
    public override void GetUnknown(StringBuilder sb) => Options.GetUnknown(sb);
}

public partial class CoffeeDrink : LineItem
{
    public override void GetUnknown(StringBuilder sb) => Options.GetUnknown(sb);
}

public partial class LatteDrink : LineItem
{
    public override void GetUnknown(StringBuilder sb) => Options.GetUnknown(sb);
}

public abstract partial class DrinkOption
{
    public virtual void GetUnknown(StringBuilder sb)
    {
        return;
    }
}

public partial class UnknownDrinkOption : DrinkOption
{
    public override void GetUnknown(StringBuilder sb)
    {
        sb.AppendLine(Text);
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
