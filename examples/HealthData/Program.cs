// Copyright (c) Microsoft. All rights reserved.
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.TypeChat;
using Microsoft.TypeChat.Schema;
using Microsoft.TypeChat.Dialog;

namespace HealthData;

public class HealthDataApp : ConsoleApp
{
    IVocabCollection _vocabs;
    JsonTranslator<HealthResponse> _translator;
    Agent<HealthResponse> _agent;

    public HealthDataApp()
    {
        _vocabs = VocabFile.Load("Vocabs.json");
        _translator = new JsonTranslator<HealthResponse>(
            new LanguageModel(Config.LoadOpenAI()),
            new TypeValidator<HealthResponse>()
        );
        _agent = new Agent<HealthResponse>(_translator);
        _agent.SaveResponse = false;
    }

    public override async Task ProcessRequestAsync(string input, CancellationToken cancelToken)
    {
        var response = await _agent.TranslateAsync(input, cancelToken);
        if (response.Value != null)
        {
            Console.WriteLine(Json.Stringify(response.Value));
        }
        if (response.Question != null &&
            !string.IsNullOrEmpty(response.Question.Text))
        {
            Console.WriteLine($"📝: {response.Question.Text}");
        }
    }

    public static async Task<int> Main(string[] args)
    {
        try
        {
            HealthDataApp app = new HealthDataApp();
            await app.RunAsync("💉💊🤧> ", args.GetOrNull(0));
        }
        catch (Exception ex)
        {
            WriteError(ex);
            return -1;
        }

        return 0;
    }
}

