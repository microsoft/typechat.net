// Copyright (c) Microsoft. All rights reserved.
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
        if (Items is not null)
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

public partial class EspressoDrinks : LineItem
{
    public override void GetUnknown(StringBuilder sb) => Options.GetUnknown(sb);
}

public partial class CoffeeDrinks : LineItem
{
    public override void GetUnknown(StringBuilder sb) => Options.GetUnknown(sb);
}

public partial class LatteDrinks : LineItem
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

internal static class CartEx
{
    public static void GetUnknown(this DrinkOption[] options, StringBuilder sb)
    {
        if (options is not null)
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
            Items =
            [
                new EspressoDrinks {Name = "espresso", Quantity = 1 },
                new CoffeeDrinks {Name = "coffee", Size = CoffeeSize.Tall, Quantity = 2}
            ]
        };
        return cart;
    }
}
