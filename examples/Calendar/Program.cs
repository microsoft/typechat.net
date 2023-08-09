// Copyright (c) Microsoft. All rights reserved.
using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.TypeChat;
using Microsoft.TypeChat.Schema;
using Microsoft.TypeChat.SemanticKernel;

namespace Calendar;

public class CalendarApp : ConsoleApp
{
    JsonTranslator<CalendarActions> _translator;

    CalendarApp()
    {
        _translator = KernelFactory.JsonTranslator<CalendarActions>(Config.LoadOpenAI());
        // Uncomment to see ALL raw messages to and from the AI
        //base.SubscribeAllEvents(_translator);
    }

    public TypeSchema Schema => _translator.Validator.Schema;

    protected override async Task ProcessRequestAsync(string input, CancellationToken cancelToken)
    {
        CalendarActions actions = await _translator.TranslateAsync(input);
        Console.WriteLine(Json.Stringify(actions));
        PrintUnknown(actions);
    }

    bool PrintUnknown(CalendarActions calendarActions)
    {
        int countUnknown = 0;
        foreach(var action in calendarActions.Actions)
        {
            if (action is UnknownAction unknown)
            {
                ++countUnknown;
                if (countUnknown == 1)
                {
                    Console.WriteLine("I didn't understand the following:");
                }
                Console.WriteLine(unknown.Text);
            }
        }
        return countUnknown > 0;
    }

    public static async Task<int> Main(string[] args)
    {
        try
        {
            CalendarApp app = new CalendarApp();
            // Un-comment to print auto-generated schema at start:
            Console.WriteLine(app.Schema.Schema.Text);
            await app.RunAsync("📅> ", args.GetOrNull(0));
        }
        catch (Exception ex)
        {
            WriteError(ex);
            return -1;
        }

        return 0;
    }
}
