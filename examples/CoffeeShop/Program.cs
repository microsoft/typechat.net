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
    TypescriptSchema _exportedSchema;
    TypeChatJsonTranslator<Cart> _service;

    CoffeeShop()
    {
        _exportedSchema = TypescriptExporter.GenerateSchema(typeof(Cart));
        _service = KernelFactory.JsonTranslator<Cart>(_exportedSchema.Schema, Config.LoadOpenAI());
        // Uncomment to see the raw reponse from the AI
        //_service.SendingPrompt += this.OnSendingPrompt;
        //_service.CompletionReceived += this.OnCompletionReceived;
    }

    public TypeSchema Schema => _exportedSchema;

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
        try
        {
            CoffeeShop app = new CoffeeShop();
            // Un-comment to print auto-generated schema at start:
            // Console.WriteLine(app.Schema.Schema.Text);

            await app.RunAsync("☕> ", args.GetOrNull(0));
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex);
            return -1;
        }

        return 0;
    }
}
