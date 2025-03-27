// Copyright (c) Microsoft. All rights reserved.
using Microsoft.TypeChat;

namespace Sentiment;

public class SentimentApp : ConsoleApp
{
    private readonly JsonTranslator<SentimentResponse> _translator;

    public SentimentApp()
    {
        OpenAIConfig config = Config.LoadOpenAI();
        // Although this sample uses config files, you can also load config from environment variables
        // OpenAIConfig config = OpenAIConfig.LoadFromJsonFile("your path");
        // OpenAIConfig config = OpenAIConfig.FromEnvironment();
        _translator = new JsonTranslator<SentimentResponse>(new LanguageModel(config));
    }

    public override async Task ProcessInputAsync(string input, CancellationToken cancelToken)
    {
        SentimentResponse response = await _translator.TranslateAsync(input, cancelToken);
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

