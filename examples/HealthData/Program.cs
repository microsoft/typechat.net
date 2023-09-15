// Copyright (c) Microsoft. All rights reserved.

using Microsoft.TypeChat;
using Microsoft.TypeChat.Schema;
using Microsoft.TypeChat.Dialog;

namespace HealthData;

public class HealthDataAgent : ConsoleApp
{
    Agent<HealthDataResponse> _agent;

    public HealthDataAgent()
    {
        _agent = new Agent<HealthDataResponse>(new LanguageModel(Config.LoadOpenAI()));
        _agent.Translator.MaxRepairAttempts = 2;
        _agent.Translator.ConstraintsValidator = new ConstraintsValidator<HealthDataResponse>();
        _agent.ResponseToMessage = (r) => (r.HasMessage) ?
                                          Message.FromAssistant(r.Message) :
                                          null;

        PromptSection section = "Ask the user pertinent questions to get all DATA required and optional for a valid JSON object.\n";
        section += "But stop asking if the user does have the answer OR does not know";
        _agent.Preamble.Add(section);
        _agent.Preamble.Append("Fix spelling mistakes, including phonetic misspellings.");
        _agent.Preamble.Append(PromptLibrary.Now());
        // Uncomment to observe prompts
        //base.SubscribeAllEvents(_agent.Translator);
    }

    public TypeSchema Schema => _agent.Translator.Validator.Schema;

    public override async Task ProcessRequestAsync(string input, CancellationToken cancelToken)
    {
        var response = await _agent.TranslateAsync(input, cancelToken);
        PrintResponse(response);
    }

    public override Task ProcessCommandAsync(string cmd, IList<string> args)
    {
        switch (cmd.ToLower())
        {
            default:
                Console.WriteLine($"Unhandled command {cmd}");
                break;

            case "history":
                foreach (var message in _agent.InteractionHistory.All())
                {
                    Console.WriteLine($"{message.Source}: {message.GetText()}");
                }
                break;

            case "clear":
                _agent.InteractionHistory.Clear();
                break;
        }
        return Task.CompletedTask;
    }

    void PrintResponse(HealthDataResponse response)
    {
        //Console.WriteLine($"IsDone: {response.IsDone}");
        if (response.Data != null)
        {
            Console.WriteLine(Json.Stringify(response.Data));
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
            HealthDataAgent app = new HealthDataAgent();
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
        Console.WriteLine("Enter medications and conditions.");
        Console.WriteLine("Commands:");
        Console.WriteLine("@history: Display history");
        Console.WriteLine("@clear: Reset history");
    }
}

