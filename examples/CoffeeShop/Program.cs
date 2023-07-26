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
    TypeSchema _typeSchema;
    TypeChatJsonTranslator<Cart> _service;

    CoffeeShop()
    {
        _typeSchema = GenerateSchema();
        _service = KernelFactory.JsonTranslator<Cart>(_typeSchema.Schema, Config.LoadOpenAI());
        // Uncomment to see the raw reponse from the AI
       // _service.CompletionReceived += this.OnCompletionReceived;
    }

    public TypeSchema Schema => _typeSchema;

    protected override async Task ProcessRequestAsync(string input, CancellationToken cancelToken)
    {
        Cart cart = await _service.TranslateAsync(input);

        string json = Json.Stringify(cart);
        Console.WriteLine(json);

        if (PrintAnyUnknown(cart) == 0)
        {
            Console.WriteLine("Success!");
        }
    }

    int PrintAnyUnknown(Cart cart)
    {
        int countUnknown = 0;
        if (cart.Items != null)
        {
            foreach (var item in cart.Items)
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
        return countUnknown;
    }

    private void OnCompletionReceived(string value)
    {
        Console.WriteLine("=== RAW RESPONSE ===");
        Console.WriteLine(value);
        Console.WriteLine("====================");
    }

    static TypeSchema GenerateSchema()
    {
        return TypescriptExporter.GenerateSchema(typeof(Cart), CoffeeShopVocabs.All());
    }

    public static async Task<int> Main(string[] args)
    {
        CoffeeShop app = new CoffeeShop();

        // Un-comment to print auto-generated schema at start:
        //Console.WriteLine(app.Schema.Schema.Text);

        await app.RunAsync("☕> ", args.GetOrNull(0));
        return 0;
    }
}
