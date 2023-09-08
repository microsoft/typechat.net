// Copyright (c) Microsoft. All rights reserved.
using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.TypeChat;
using Microsoft.TypeChat.Schema;

namespace Restaurant;

public class RestaurantApp : ConsoleApp
{
    JsonTranslator<Order> _translator;

    public RestaurantApp()
    {
        _translator = new JsonTranslator<Order>(
            new LanguageModel(Config.LoadOpenAI()),
            new TypeValidator<Order>()
        );
        _translator.MaxRepairAttempts = 3;
        // Uncomment to see ALL raw messages to and from the AI
        //base.SubscribeAllEvents(_translator);
    }

    public TypeSchema Schema => _translator.Validator.Schema;

    public override async Task ProcessRequestAsync(string input, CancellationToken cancelToken)
    {
        Order order = await _translator.TranslateAsync(input, cancelToken);
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
            RestaurantApp app = new RestaurantApp();
            // Un-comment to print auto-generated schema at start:
            // Console.WriteLine(app.Schema.Schema.Text);

            await app.RunAsync("🍕> ", args.GetOrNull(0));
        }
        catch (Exception ex)
        {
            WriteError(ex);
            Console.ReadLine();
            return -1;
        }

        return 0;
    }
}
