// Copyright (c) Microsoft. All rights reserved.

using Microsoft.TypeChat;
using Sentiment;
using CoffeeShop;
using Calendar;
using Restaurant;
using HealthData;

namespace SchemaHierarchy;

/// <summary>
/// </summary>
public class SchemaHierarchyApp : ConsoleApp
{
    LanguageModel _model;
    HierarchicalJsonTranslator _translator;

    public SchemaHierarchyApp()
    {
        var model = new LanguageModel(Config.LoadOpenAI());
        var embeddingModel = new TextEmbeddingModel(Config.LoadOpenAI("OpenAI_Embedding"));
        _translator = new HierarchicalJsonTranslator(model, embeddingModel);
    }

    public override async Task ProcessInputAsync(string input, CancellationToken cancelToken)
    {
        object result = await _translator.TranslateToObjectAsync(input, cancelToken);
        if (result != null)
        {
            Console.WriteLine($"{result.GetType()}");
            Console.WriteLine(Json.Stringify(result));
        }
    }

    protected override async Task InitApp()
    {
        Console.WriteLine("Initializing app");
        // While this is hardcoded here, you can also do this dynamically
        // targets dynamically
        await _translator.AddSchemaAsync<Cart>("Order Coffee Drinks (Italian names included) and Baked Goods");
        await _translator.AddSchemaAsync<CalendarActions>("Actions related to calendars, appointments, meetings, schedules");
        await _translator.AddSchemaAsync<Order>("Order pizza, beer and salads");
        await _translator.AddSchemaAsync<SentimentResponse>("Statements with sentiments, emotions, feelings, impressions about places, things, the surroundings");
        await _translator.AddSchemaAsync<HealthDataResponse>("Health information: enter your medications, conditions");
    }

    public static async Task<int> Main(string[] args)
    {
        try
        {
            SchemaHierarchyApp app = new SchemaHierarchyApp();
            await app.RunAsync("☕📅🍕💊🤧😀> ", args.GetOrNull(0));
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

