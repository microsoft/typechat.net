// Copyright (c) Microsoft. All rights reserved.

using System.Text;

using Microsoft.TypeChat;
using Microsoft.TypeChat.Schema;

namespace CoffeeShop;

public class CoffeeShopApp : ConsoleApp
{
    private readonly JsonTranslator<Cart> _translator;

    public CoffeeShopApp()
    {
        _translator = new JsonTranslator<Cart>(
            new LanguageModel(Config.LoadOpenAI())
        )
        {
            MaxRepairAttempts = 3
        };
        // Uncomment to see ALL raw messages to and from the AI
        //base.SubscribeAllEvents(_translator);
    }

    public TypeSchema Schema => _translator.Validator.Schema;

    public override async Task ProcessInputAsync(string input, CancellationToken cancelToken)
    {
        Cart cart = await _translator.TranslateAsync(input, cancelToken);

        Console.WriteLine("##YOUR ORDER");
        string json = Microsoft.TypeChat.Json.Stringify(cart);
        Console.WriteLine(json);

        if (!PrintAnyUnknown(cart))
        {
            Console.WriteLine("Success!");
        }
    }

    private bool PrintAnyUnknown(Cart cart)
    {
        if (cart.Items is not null)
        {
            StringBuilder sb = new StringBuilder();
            cart.GetUnknown(sb);
            if (sb.Length > 0)
            {
                Console.WriteLine();
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
            CoffeeShopApp app = new CoffeeShopApp();
            // Un-comment to print auto-generated schema at start:
            //Console.WriteLine(app.Schema.Schema.Text);

            await app.RunAsync("☕> ", args.GetOrNull(0));
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
