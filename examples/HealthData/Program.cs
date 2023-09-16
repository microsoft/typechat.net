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
        //
        // We will only capture the questions the model asked us in the history
        //
        _agent.TransformResponseForHistory = (r) => (r.HasMessage) ?
                                          Message.FromAssistant(r.Message) :
                                          null;
        _agent.Preamble.Append(PromptLibrary.Now());
        _agent.Preamble.Append(GetInstructions());
        // Uncomment to observe prompts
        //base.SubscribeAllEvents(_agent.Translator);
    }

    public TypeSchema Schema => _agent.Translator.Validator.Schema;

    public override async Task ProcessRequestAsync(string input, CancellationToken cancelToken)
    {
        Prompt request = new Prompt();
        request += input;
        var response = await _agent.TranslateAsync(request, cancelToken);
        PrintResponse(response);
    }

    PromptSection GetInstructions()
    {
        return "Help me enter my health data step by step.\n" +
               "Ask specific questions to gather required OR optional fields I have not already provided" +
               "Stop asking if I don't know the answer\n" +
               "Automatically fix my spelling mistakes\n" +
               "Always return a response";
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

