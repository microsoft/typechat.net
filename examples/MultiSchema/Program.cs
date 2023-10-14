// Copyright (c) Microsoft. All rights reserved.

using Calendar;
using CoffeeShop;
using HealthData;
using Math;
using Microsoft.TypeChat;
using Microsoft.TypeChat.Classification;
using Plugins;
using Restaurant;
using Sentiment;

namespace MultiSchema;

/// <summary>
/// A sample app that demonsrates how to route user intent to multiple target schemas and/or apps
/// This auto-wraps the other example applications 
/// </summary>
public class MultiSchemaApp : ConsoleApp
{
    ILanguageModel _model;
    TextRequestRouter<IInputHandler> _childApps;

    public MultiSchemaApp()
    {
        _model = new LanguageModel(Config.LoadOpenAI());
        _childApps = new TextRequestRouter<IInputHandler>(_model);
        InitApps();
    }

    public override async Task ProcessInputAsync(string input, CancellationToken cancelToken)
    {
        var match = await _childApps.ClassifyRequestAsync(input, cancelToken);
        var processor = match.Value;
        if (processor != null)
        {
            Console.WriteLine($"App Selected: {match.Key}");
            await processor.ProcessInputAsync(input, cancelToken);
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
        _childApps.Add(new CoffeeShopApp(), "CoffeeShop", "Order Coffee Drinks (Italian names included) and Baked Goods");
        _childApps.Add(new CalendarApp(), "Calendar", "Actions related to calendars, appointments, meetings, schedules");
        _childApps.Add(new RestaurantApp(), "Restaurant", "Order pizza, beer and salads");
        _childApps.Add(new MathApp(), "Math", "Calculations using the four basic math operations");
        _childApps.Add(new SentimentApp(), "Sentiment", "Statements with sentiments, emotions, feelings, impressions about places, things, the surroundings");
        _childApps.Add(new PluginApp(), "Plugins", "Command shell operations: list and search for files, machine information");
        _childApps.Add(new HealthDataAgent(), "HealthData", "Health information: enter your medications, conditions");
        _childApps.Add(null, "No Match", "None of the others matched");
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

