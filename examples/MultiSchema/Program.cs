// Copyright (c) Microsoft. All rights reserved.
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.TypeChat;
using Microsoft.TypeChat.Schema;

using Sentiment;
using CoffeeShop;
using Calendar;
using Restaurant;
using Math;

namespace MultiSchema;

/// <summary>
/// A sample app that demonsrates how to route user intent to multiple target schemas and/or apps
/// This auto-wraps the other example applications 
/// </summary>
public class MultiSchemaApp : ConsoleApp
{
    ILanguageModel _model;
    TextClassifier _appClassifier;
    Dictionary<string, IIntentProcessor> _childApps;

    public MultiSchemaApp()
    {
        _model = new LanguageModel(Config.LoadOpenAI());
        InitApps();
    }

    public override async Task ProcessRequestAsync(string input, CancellationToken cancelToken)
    {
        TextClassification appToUse = await _appClassifier.TranslateAsync(input, cancelToken);
        // Route input to the target app
        IIntentProcessor? processor = (appToUse.Class != null) ?
                                    _childApps.GetValueOrDefault(appToUse.Class) :
                                    null;
        if (processor != null)
        {
            Console.WriteLine($"App Selected: {appToUse.Class}");
            await processor.ProcessRequestAsync(input, cancelToken);
        }
        else
        {
            Console.WriteLine("No suitable app found to handle your request.");
            PrintApps();
        }
    }

    void InitApps()
    {
        // While this is hardcoded here, you can also do this dynamically
        // targets dynamically
        _appClassifier = new TextClassifier(_model);
        _childApps = new Dictionary<string, IIntentProcessor>(StringComparer.OrdinalIgnoreCase);

        _childApps.Add("CoffeeShop", new CoffeeShopApp());
        _appClassifier.Classes.Add("CoffeeShop", "Order Coffee Drinks (Italian names included) and Baked Goods");

        _childApps.Add("Calendar", new CalendarApp());
        _appClassifier.Classes.Add("Calendar", "Actions related to calendars, appointments, meetings, schedules");

        _childApps.Add("Restaurant", new RestaurantApp());
        _appClassifier.Classes.Add("Restaurant", "Order pizza, beer and salads");

        _childApps.Add("Math", new MathApp());
        _appClassifier.Classes.Add("Math", "Calculations using the four basic math operations");

        _childApps.Add("Sentiment", new SentimentApp());
        _appClassifier.Classes.Add("Sentiment", "Statements with sentiments, emotions, feelings, impressions about places, things, the surroundings");

        _appClassifier.Classes.Add("No Match", "None of the others matched");
    }

    void PrintApps()
    {
        foreach (var app in _appClassifier.Classes)
        {
            if (_childApps.ContainsKey(app.Name))
            {
                Console.WriteLine(app);
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
            return -1;
        }

        return 0;
    }
}

