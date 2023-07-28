// Copyright (c) Microsoft. All rights reserved.
using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.TypeChat;
using Microsoft.TypeChat.Schema;
using Microsoft.TypeChat.SemanticKernel;

namespace CoffeeShop;

public class CoffeeShop : ConsoleApp
{
    IVocabCollection _vocabs;
    TypescriptSchema _typeSchema;
    TypeChatJsonTranslator<Cart> _service;

    CoffeeShop()
    {
        _vocabs = CoffeeShopVocabs.All();
        _typeSchema = TypescriptExporter.GenerateSchema(typeof(Cart), _vocabs);
        _service = KernelFactory.JsonTranslator<Cart>(_typeSchema.Schema, Config.LoadOpenAI());
        _service.Validator = new TypeValidator<Cart>(_typeSchema);
        // Uncomment to see the raw reponse from the AI
        _service.CompletionReceived += this.OnCompletionReceived;
    }

    public TypeSchema Schema => _typeSchema;

    protected override async Task ProcessRequestAsync(string input, CancellationToken cancelToken)
    {
        Cart cart = await _service.TranslateAsync(input);

        string json = Json.Stringify(cart);
        Console.WriteLine(json);

        if (!PrintAnyUnknown(cart))
        {
            Console.WriteLine("Success!");
        }
    }

    bool PrintAnyUnknown(Cart cart)
    {
        if (cart.Items != null)
        {
            StringBuilder sb = new StringBuilder();
            cart.GetUnknown(sb);
            if (sb.Length > 0)
            {
                Console.WriteLine("I didn't understand the following:");
                Console.WriteLine(sb.ToString());
                return true;
            }
        }
        return false;
    }

    public static async Task<int> Main(string[] args)
    {
        CoffeeShop app = new CoffeeShop();

        // Un-comment to print auto-generated schema at start:
        Console.WriteLine(app.Schema.Schema.Text);

        await app.RunAsync("☕> ", args.GetOrNull(0));

        return 0;
    }
}
