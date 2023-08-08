// Copyright (c) Microsoft. All rights reserved.
using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.TypeChat;
using Microsoft.TypeChat.Schema;
using Microsoft.TypeChat.SemanticKernel;

namespace Restaurant;

public class Restaurant : ConsoleApp
{
    JsonTranslator<Order> _translator;

    Restaurant()
    {
        _translator = KernelFactory.JsonTranslator<Order>(Config.LoadOpenAI());
        _translator.MaxRepairAttempts = 3;
        // Uncomment to see ALL raw messages to and from the AI
        //base.SubscribeAllEvents(_translator);
    }

    public TypeSchema Schema => _translator.Validator.Schema;

    protected override async Task ProcessRequestAsync(string input, CancellationToken cancelToken)
    {
        Order order = await _translator.TranslateAsync(input);

        string json = Json.Stringify(order);
        Console.WriteLine(json);

        if (!PrintAnyUnknown(order))
        {
            Console.WriteLine("Success!");
        }
    }

    bool PrintAnyUnknown(Order order)
    {
        if (order.Items != null)
        {
            StringBuilder sb = new StringBuilder();
            order.GetUnknown(sb);
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
            Restaurant app = new Restaurant();
            // Un-comment to print auto-generated schema at start:
            Console.WriteLine(app.Schema.Schema.Text);

            await app.RunAsync("🍕> ", args.GetOrNull(0));
        }
        catch (Exception ex)
        {
            WriteError(ex);
            return -1;
        }

        return 0;
    }
}
