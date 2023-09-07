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
    JsonTranslator<HealthDataResponse> _translator;
    Agent<HealthDataResponse> _agent;

    public HealthDataApp()
    {
        _vocabs = VocabFile.Load("Vocabs.json");
        _translator = new JsonTranslator<HealthDataResponse>(
            new LanguageModel(Config.LoadOpenAI()),
            new TypeValidator<HealthDataResponse>(_vocabs)
        );
        _agent = new Agent<HealthDataResponse>(_translator);
        _agent.SaveResponse = false;
        _agent.Preamble.PushInstruction($"Ask questions you have ALL information required for {nameof(Medication)}");
    }

    public override async Task ProcessRequestAsync(string input, CancellationToken cancelToken)
    {
        var response = await _agent.TranslateAsync(input, cancelToken);
        if (response.HasQuestion)
        {
            _agent.InteractionHistory.Append(Message.FromAssistant(response.Question.Text));
        }
        PrintResponse(response);
    }

    void PrintResponse(HealthDataResponse response)
    {
        Console.WriteLine($"IsDone: {response.IsDone}");
        if (response.Value != null)
        {
            Console.WriteLine(Json.Stringify(response.Value));
        }
        if (response.HasQuestion)
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

