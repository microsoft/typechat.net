// Copyright (c) Microsoft. All rights reserved.

using Microsoft.TypeChat;
using Microsoft.TypeChat.Dialog;
using Microsoft.TypeChat.Schema;

namespace HealthData;

public class HealthDataAgent : ConsoleApp
{
    AgentWithHistory<HealthDataResponse> _agent;

    public HealthDataAgent()
    {
        // Create an agent with history
        _agent = new AgentWithHistory<HealthDataResponse>(new ChatLanguageModel(Config.LoadOpenAI()));
        // Instruct the agent on how it should act
        GiveAgentInstructions();
        // We only capture the questions that the model asked us into history
        _agent.CreateMessageForHistory = (r) => (r.HasMessage) ? Message.FromAssistant(r.Message) : null;
        // Enforce additional constraints validation, as provided by System.ComponentModel.DataAnnotations namespace
        _agent.Translator.ConstraintsValidator = new ConstraintsValidator<HealthDataResponse>();
        //
        // Set up some parameters
        //
        _agent.MaxRequestPromptLength = 2048; // Limit # of characters, to avoid hitting token limits
        _agent.Translator.MaxRepairAttempts = 2;

        // Uncomment to observe prompts
        //base.SubscribeAllEvents(_agent.Translator);
    }

    public TypeSchema Schema => _agent.Translator.Validator.Schema;

    public override async Task ProcessInputAsync(string input, CancellationToken cancelToken)
    {
        HealthDataResponse response = await _agent.GetResponseAsync(input, cancelToken);
        PrintResponse(response);
    }

    /// <summary>
    /// Give the agent instructions
    /// </summary>
    void GiveAgentInstructions()
    {
        Prompt instructions = _agent.Instructions;
        // Supply current date and time, and how to use it
        instructions += PromptLibrary.Now();
        instructions += "Help me enter my health data step by step.\n" +
               "Ask specific questions to gather required and optional fields I have not already provided" +
               "Stop asking if I don't know the answer\n" +
               "Automatically fix my spelling mistakes\n" +
               "My health data may be complex: always record and return ALL of it.\n" +
               "Always return a response:\n" +
               "- If you don't understand what I say, ask a question.\n" +
               "- At least respond with an OK message.";
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
                Console.Clear();
                PrintHelp();
                break;
        }
        return Task.CompletedTask;
    }

    void PrintResponse(HealthDataResponse response)
    {
        if (response.Data is not null)
        {
            Console.WriteLine(Microsoft.TypeChat.Json.Stringify(response.Data));
        }
        if (response.HasMessage)
        {
            Console.WriteLine($"📝: {response.Message}");
        }
        if (response.HasNotTranslated)
        {
            Console.WriteLine($"🤔: I did not understand\n {response.NotTranslated}");
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

