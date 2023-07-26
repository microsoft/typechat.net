// Copyright (c) Microsoft. All rights reserved.
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.TypeChat;
using Microsoft.TypeChat.Schema;
using Microsoft.TypeChat.SemanticKernel;

namespace CoffeeShop;

public class CoffeeShop : ConsoleApp
{
    TypeChatJsonTranslator<Cart> _service;

    CoffeeShop()
    {
        _service = KernelFactory.JsonTranslator<Cart>(Config.LoadOpenAI());
        _service.CompletionReceived += this.OnCompletionReceived;
    }

    protected override async Task ProcessRequestAsync(string input, CancellationToken cancelToken)
    {
        Cart cart = await _service.TranslateAsync(input);
        string json = Json.Stringify(cart);
        Console.WriteLine(json);
    }

    private void OnCompletionReceived(string value)
    {
        Console.WriteLine(value);
    }

    static TypeSchema GetSchema()
    {
        return TypescriptExporter.GenerateSchema(typeof(Cart), CoffeeShopVocabs.All());
    }

    static Cart TestCart()
    {
        Cart cart = new Cart
        {
            Items = new CartItem[]
            {
                new LineItem
                {
                    Product = new EspressoDrink {Name = "espresso" },
                    Quantity = 1
                },
                new LineItem
                {
                    Product = new CoffeeDrink {Name = "coffee", Size = CoffeeSize.Tall},
                    Quantity = 2
                }
            }
        };
        return cart;
    }

    public static async Task<int> Main(string[] args)
    {
        var cart = TestCart();
        string json = Json.Stringify(cart);
        Console.WriteLine(json);

        var schema = GetSchema();
        Console.WriteLine(schema.Schema.Text);

        CoffeeShop app = new CoffeeShop();
        await app.RunAsync("☕> ", args.GetOrNull(2));
        return 0;
    }
}
