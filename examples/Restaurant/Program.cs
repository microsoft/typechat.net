// Copyright (c) Microsoft. All rights reserved.
using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.TypeChat;
using Microsoft.TypeChat.Schema;

namespace Restaurant;

public class Restaurant : ConsoleApp
{
    JsonTranslator<Order> _translator;

    Restaurant()
    {
        _translator = new JsonTranslator<Order>(new CompletionService(Config.LoadOpenAI()), new TypeValidator<Order>());
        _translator.MaxRepairAttempts = 3;
        // Uncomment to see ALL raw messages to and from the AI
        //base.SubscribeAllEvents(_translator);
    }

    public TypeSchema Schema => _translator.Validator.Schema;

    protected override async Task ProcessRequestAsync(string input, CancellationToken cancelToken)
    {
        Order order = await _translator.TranslateAsync(input);
        PrintOrder(order);
    }

    void PrintOrder(Order order)
    {
        if (order.Items != null)
        {
            (string printedOrder, string log) = order.ProcessOrder();
            Console.WriteLine();
            Console.WriteLine("### Your order");
            Console.WriteLine(printedOrder);
            Console.WriteLine(log);
        }
    }

    public static async Task<int> Main(string[] args)
    {
        try
        {
            Restaurant app = new Restaurant();
            // Un-comment to print auto-generated schema at start:
            // Console.WriteLine(app.Schema.Schema.Text);

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
