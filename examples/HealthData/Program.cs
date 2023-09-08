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
    Agent<HealthDataResponse> _agent;

    public HealthDataApp()
    {
        _agent = new Agent<HealthDataResponse>(new LanguageModel(Config.LoadOpenAI()));
        _agent.SaveResponse = false;

        PromptSection preamble = "Ask the user questions until you have all data required by the JSON object. ";
        preamble += "Until then, return a null object. ";
        preamble += "Always fix spelling mistakes; phonetic misspellings are common";
        _agent.Preamble.Push(preamble);
        _agent.Preamble.Push(PromptLibrary.Now());
        // Uncomment to observe prompts
        //base.SubscribeAllEvents(_agent.Translator);
    }

    public TypeSchema Schema => _agent.Translator.Validator.Schema;

    public override async Task ProcessRequestAsync(string input, CancellationToken cancelToken)
    {
        var response = await _agent.TranslateAsync(input, cancelToken);
        if (response.HasMessage)
        {
            _agent.InteractionHistory.Append(Message.FromAssistant(response.Message));
        }
        PrintResponse(response);
    }

    public override Task ProcessCommandAsync(string cmd, IList<string> args)
    {
        switch(cmd.ToLower())
        {
            default:
                Console.WriteLine($"Unhandled command {cmd}");
                break;

            case "clear":
                _agent.InteractionHistory.Clear();
                break;
        }
        return Task.CompletedTask;
    }

    void PrintResponse(HealthDataResponse response)
    {
        Console.WriteLine($"IsDone: {response.IsDone}");
        if (response.Value != null)
        {
            Console.WriteLine(Json.Stringify(response.Value));
        }
        if (response.HasMessage)
        {
            Console.WriteLine($"📝: {response.Message}");
        }
    }

    public static async Task<int> Main(string[] args)
    {
        try
        {
            HealthDataApp app = new HealthDataApp();
            //Console.WriteLine(app.Schema.Schema.Text);
            PrintHelp();
            await app.RunAsync("💉💊🤧> ", args.GetOrNull(0));
        }
        catch (Exception ex)
        {
            WriteError(ex);
            Console.ReadLine();
            return -1;
        }

        return 0;
    }

    static void PrintHelp()
    {
        Console.WriteLine("Enter medications and conditions");
        Console.WriteLine("@clear:\tReset history");
    }
}

