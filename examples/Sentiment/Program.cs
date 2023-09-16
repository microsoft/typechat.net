// Copyright (c) Microsoft. All rights reserved.
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.TypeChat;
using Microsoft.TypeChat.Schema;

namespace Sentiment;

public class SentimentApp : ConsoleApp
{
    JsonTranslator<SentimentResponse> _translator;

    public SentimentApp()
    {
        _translator = new JsonTranslator<SentimentResponse>(new LanguageModel(Config.LoadOpenAI()));
    }

    public override async Task ProcessInputAsync(string input, CancellationToken cancelToken)
    {
        SentimentResponse response = await _translator.TranslateAsync(input);
        Console.WriteLine($"The sentiment is {response.Sentiment}");
    }

    public static async Task<int> Main(string[] args)
    {
        try
        {
            SentimentApp app = new SentimentApp();
            await app.RunAsync("😀> ", args.GetOrNull(0));
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

