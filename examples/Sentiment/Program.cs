// Copyright (c) Microsoft. All rights reserved.
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.TypeChat;
using Microsoft.TypeChat.SemanticKernel;

namespace Sentiment;

public class SentimentResponse
{
    [JsonPropertyName("sentiment")]
    public string Sentiment { get; set; }
}

public class SentimentApp : ConsoleApp
{
    TypeChatJsonTranslator<SentimentResponse> _translator;

    public SentimentApp()
    {
        _translator = KernelFactory.JsonTranslator<SentimentResponse>(Config.LoadOpenAI());
    }

    protected override async Task ProcessRequestAsync(string input, CancellationToken cancelToken)
    {
        SentimentResponse response = await _translator.TranslateAsync(input);
        Console.WriteLine($"The sentiment is {response.Sentiment}");
    }

    public static async Task<int> Main(string[] args)
    {
        SentimentApp app = new SentimentApp();
        await app.RunAsync("😀> ", args.GetOrNull(0));
        return 0;
    }
}

