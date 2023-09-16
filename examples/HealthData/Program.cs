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
        _agent.Instructions.Append(PromptLibrary.Now());
        _agent.Instructions.Append(GetInstructions());
        // Uncomment to observe prompts
        //base.SubscribeAllEvents(_agent.Translator);
    }

    public TypeSchema Schema => _agent.Translator.Validator.Schema;

    public override async Task ProcessInputAsync(string input, CancellationToken cancelToken)
    {
        var response = await _agent.TranslateAsync(input, cancelToken);
        PrintResponse(response);
    }

    PromptSection GetInstructions()
    {
        return "Help me enter my health data step by step.\n" +
               "Ask specific questions to gather required OR optional fields I have not already provided" +
               "Stop asking if I don't know the answer\n" +
               "Automatically fix my spelling mistakes\n" +
               "My health data may be complex: always record and return ALL of it.\n" +
               "Always return a response. If you don't understand what I say, ask a question.";
    }

    public override Task ProcessCommandAsync(string cmd, IList<string> args)
    {
        switch (cmd.ToLower())
        {
            default:
                Console.WriteLine($"Unhandled command {cmd}");
                break;

            case "history":
                foreach (var message in _agent.History.All())
                {
                    Console.WriteLine($"{message.Source}: {message.GetText()}");
                }
                break;

            case "clear":
                _agent.History.Clear();
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

