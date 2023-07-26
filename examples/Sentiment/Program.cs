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
    TypeChatJsonTranslator<SentimentResponse> _service;

    public SentimentApp()
    {
        _service = KernelFactory.JsonTranslator<SentimentResponse>(Config.LoadOpenAI());
    }

    protected override async Task ProcessRequestAsync(string line, CancellationToken cancelToken)
    {
        SentimentResponse response = await _service.TranslateAsync(line);
        await WriteLineAsync($"The sentiment is {response.Sentiment}");
    }

    public static async Task<int> Main(string[] args)
    {
        SentimentApp app = new SentimentApp();
        await app.RunAsync();
        return 0;
    }
}

