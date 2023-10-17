// Copyright (c) Microsoft. All rights reserved.

using Calendar;
using CoffeeShop;
using HealthData;
using Microsoft.TypeChat;
using Restaurant;
using Sentiment;

namespace SchemaHierarchy;

public class SchemaHierarchyApp : ConsoleApp
{
    private HierarchicalJsonTranslator _translator;

    public SchemaHierarchyApp()
    {
        var model = new LanguageModel(Config.LoadOpenAI());
        var embeddingModel = new TextEmbeddingModel(Config.LoadOpenAI("OpenAI_Embedding"));
        _translator = new HierarchicalJsonTranslator(model, embeddingModel);
    }

    public override async Task ProcessInputAsync(string input, CancellationToken cancelToken = default)
    {
        object result = await _translator.TranslateToObjectAsync(input, cancelToken);
        if (result is not null)
        {
            Console.WriteLine($"{result.GetType()}");
            Console.WriteLine(Json.Stringify(result));
        }
    }

    protected override async Task InitAppAsync(CancellationToken cancelToken)
    {
        Console.WriteLine("Initializing app");
        // While this is hardcoded here, you can also do this dynamically
        // targets dynamically
        await _translator.AddSchemaAsync<Cart>("Order Coffee Drinks (Italian names included) and Baked Goods", cancelToken);
        await _translator.AddSchemaAsync<CalendarActions>("Actions related to calendars, appointments, meetings, schedules", cancelToken);
        await _translator.AddSchemaAsync<Order>("Order pizza, beer and salads", cancelToken);
        await _translator.AddSchemaAsync<SentimentResponse>("Statements with sentiments, emotions, feelings, impressions about places, things, the surroundings", cancelToken);
        await _translator.AddSchemaAsync<HealthDataResponse>("Health information: enter your medications, conditions", cancelToken);
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

