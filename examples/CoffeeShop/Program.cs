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
        //_service.CompletionReceived += this.OnCompletionReceived;
    }

    protected override async Task ProcessRequestAsync(string input, CancellationToken cancelToken)
    {
        Cart cart = await _service.TranslateAsync(input);
        string json = Json.Stringify(cart);
        PrintAnyUnknown(cart);
        Console.WriteLine(json);
    }

    void PrintAnyUnknown(Cart cart)
    {
        if (cart.Items == null)
        {
            return;
        }
        int countUnknown = 0;
        foreach(var item in cart.Items)
        {
            if (item is UnknownItem unknown)
            {
                if (countUnknown == 0)
                {
                    Console.WriteLine("I didn't understand the following:");
                }
                Console.WriteLine(unknown.Text);
                ++countUnknown;
            }
        }
    }

    private void OnCompletionReceived(string value)
    {
        Console.WriteLine(value);
    }

    static TypeSchema GetSchema()
    {
        return TypescriptExporter.GenerateSchema(typeof(Cart), CoffeeShopVocabs.All());
    }

    public static async Task<int> Main(string[] args)
    {
        CoffeeShop app = new CoffeeShop();
        await app.RunAsync("☕> ", args.GetOrNull(2));
        return 0;
    }
}
