// Copyright (c) Microsoft. All rights reserved.
using System.Text;

namespace Restaurant;

/**
 * We use partial classes to deliberately keep SCHEMA separate from implementation
 * CoffeeShopSchema.cs defines the shape of the data
 * CoffeeShopImpl.cs has the code. 
 **/

public partial class Order
{
    public virtual void GetUnknown(StringBuilder sb)
    {
        if (Items != null)
        {
            foreach (OrderItem item in Items)
            {
                item.GetUnknown(sb);
            }
        }
    }
}

public abstract partial class OrderItem
{
    public virtual void GetUnknown(StringBuilder sb) { return; }
}

public partial class UnknownItem : OrderItem
{
    public override void GetUnknown(StringBuilder sb)
    {
        sb.AppendLine(Text);
    }
}
