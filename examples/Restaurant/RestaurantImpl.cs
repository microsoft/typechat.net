// Copyright (c) Microsoft. All rights reserved.
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.TypeChat;

namespace Restaurant;

/**
 * We use partial classes to deliberately keep SCHEMA separate from implementation
 * CoffeeShopSchema.cs defines the shape of the data
 * CoffeeShopImpl.cs has the code. 
 **/

public partial class Order
{
    public (string, string) ProcessOrder()
    {
        StringBuilder order = new StringBuilder();
        StringBuilder log = new StringBuilder();

        if (Items != null)
        {
            for (int i = 0; i < Items.Length; ++i)
            {
                var item = Items[i];
                item.Process();
                order.Append($"[{i + 1}]: ");
                item.Print(order, log);
                order.AppendLine();
            }
        }
        return (order.ToString(), log.ToString());
    }
}

public abstract partial class OrderItem
{
    public virtual void Process() { return; }
    public virtual void Print(StringBuilder order, StringBuilder log) { return; }

    protected string[] ConcatItems(string[]? items, IVocab vocab)
    {
        IEnumerable<string> newItems = (items != null) ?
                                        items.Concat(vocab.Strings()) :
                                        vocab.Strings();
        return newItems.Distinct().ToArray();
    }

    protected string[] RemoveItems(string[] items, string[] itemsToRemove)
    {
        var newItems = from item in items
                       where !itemsToRemove.Contains(item)
                       select item;
        return newItems.ToArray();
    }

    protected void PrintAddedItems(string[] addedItems, IVocab vocab, StringBuilder order, StringBuilder log)
    {
        if (addedItems != null && addedItems.Length > 0)
        {
            int toppingCount = 0;
            foreach (var addition in addedItems)
            {
                if (vocab.Contains(addition, StringComparison.OrdinalIgnoreCase))
                {
                    toppingCount++;
                    order.Append((toppingCount == 1) ? " with " : ", ");
                    order.Append(addition);
                }
                else
                {
                    log.AppendLine($"We are out of {addition}");
                }
            }
        }
    }

    protected void PrintRemovedItems(string[] removedItems, bool hasAdded, StringBuilder order, StringBuilder log)
    {
        if (removedItems != null && removedItems.Length > 0)
        {
            for (int i = 0; i < removedItems.Length; ++i)
            {
                order.Append(i > 0 ? ", " : hasAdded ? " and without " : " without ");
                order.Append(removedItems[i]);
            }
        }
    }
}

public partial class UnknownItem : OrderItem
{
    public override void Print(StringBuilder order, StringBuilder log)
    {
        log.Append("Unrecognized item: ").AppendLine(Text);
    }
}

public partial class Pizza : LineItem
{
    [JsonIgnore]
    public bool HasToppings => (AddedToppings != null && AddedToppings.Length > 0);

    public override void Process()
    {
        if (string.IsNullOrEmpty(Name))
        {
            return;
        }
        Size ??= "large";
        var vocabType = RestaurantVocabs.NamedPizzas.Get(Name);
        if (vocabType != null)
        {
            AddedToppings = ConcatItems(AddedToppings, vocabType.Vocab);
        }
        if (RemovedToppings != null)
        {
            RemoveItems(AddedToppings, RemovedToppings);
        }
    }

    public override void Print(StringBuilder order, StringBuilder log)
    {
        IVocab vocab = RestaurantVocabs.AvailableIngredients.Get("Pizza").Vocab;
        order.Append($"{Quantity} {Size} Pizza");
        PrintAddedItems(AddedToppings, vocab, order, log);
        PrintRemovedItems(RemovedToppings, HasToppings, order, log);
    }
}

public partial class Salad : LineItem
{
    [JsonIgnore]
    public bool HasAdditions => (AddedIngredients != null && AddedIngredients.Length > 0);

    public override void Print(StringBuilder order, StringBuilder log)
    {
        IVocab vocab = RestaurantVocabs.AvailableIngredients.Get("Salad").Vocab;
        order.Append($"{Quantity} {Portion} {Style} Salad");
        PrintAddedItems(AddedIngredients, vocab, order, log);
        PrintRemovedItems(RemovedIngredients, HasAdditions, order, log);
    }
}

public partial class Beer : LineItem
{
    public override void Print(StringBuilder order, StringBuilder log)
    {
        string kind = Kind ?? "Unknown";
        order.Append($"{Quantity} Beer, Type: {kind}");
    }
}
