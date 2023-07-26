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
        _service.CompletionReceived += this.OnCompletionReceived;
    }

    public TypeSchema Schema => _typeSchema;

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

    static TypeSchema GenerateSchema()
    {
        return TypescriptExporter.GenerateSchema(typeof(Cart), CoffeeShopVocabs.All());
    }

    public static async Task<int> Main(string[] args)
    {
        CoffeeShop app = new CoffeeShop();
        Console.WriteLine(app.Schema.Schema.Text);
        await app.RunAsync("☕> ", args.GetOrNull(2));
        return 0;
    }
}
