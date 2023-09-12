// Copyright (c) Microsoft. All rights reserved.

using Microsoft.TypeChat;

using Sentiment;
using CoffeeShop;
using Calendar;
using Restaurant;
using Math;
using Plugins;

namespace MultiSchema;

/// <summary>
/// A sample app that demonsrates how to route user intent to multiple target schemas and/or apps
/// This auto-wraps the other example applications 
/// </summary>
public class MultiSchemaApp : ConsoleApp
{
    ILanguageModel _model;
    IntentRouter<IIntentProcessor> _childApps;

    public MultiSchemaApp()
    {
        _model = new LanguageModel(Config.LoadOpenAI());
        _childApps = new IntentRouter<IIntentProcessor>(_model);
        InitApps();
    }

    public override async Task ProcessRequestAsync(string input, CancellationToken cancelToken)
    {
        var match = await _childApps.RouteAsync(input, cancelToken);
        var processor = match.Value;
        if (processor != null)
        {
            Console.WriteLine($"App Selected: {match.Key}");
            await processor.ProcessRequestAsync(input, cancelToken);
        }
        else
        {
            Console.WriteLine("No suitable app found to handle your request.");
            Console.WriteLine();
            Console.WriteLine("Available apps:");
            PrintApps();
        }
    }

    void InitApps()
    {
        // While this is hardcoded here, you can also do this dynamically
        // targets dynamically
        _childApps.Add("CoffeeShop", new CoffeeShopApp(), "Order Coffee Drinks (Italian names included) and Baked Goods");
        _childApps.Add("Calendar", new CalendarApp(), "Actions related to calendars, appointments, meetings, schedules");
        _childApps.Add("Restaurant", new RestaurantApp(), "Order pizza, beer and salads");
        _childApps.Add("Math", new MathApp(), "Calculations using the four basic math operations");
        _childApps.Add("Sentiment", new SentimentApp(), "Statements with sentiments, emotions, feelings, impressions about places, things, the surroundings");
        _childApps.Add("Plugins", new PluginApp(), "Command shell operations: list and search for files, machine information");
        _childApps.Add("No Match", null, "None of the others matched");
    }

    void PrintApps()
    {
        int i = 0;
        foreach (var app in _childApps.Routes)
        {
            if (app.Value != null)
            {
                Console.WriteLine($"{++i}: {app.Key}");
            }
        }
    }

    public static async Task<int> Main(string[] args)
    {
        try
        {
            MultiSchemaApp app = new MultiSchemaApp();
            await app.RunAsync("🔀> ", args.GetOrNull(0));
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

