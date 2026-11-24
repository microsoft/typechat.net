// Copyright (c) Microsoft. All rights reserved.

using Microsoft.TypeChat;
using Microsoft.TypeChat.Schema;

namespace Calendar;

public class CalendarApp : ConsoleApp
{
    private readonly JsonTranslator<CalendarActions> _translator;
    private readonly TimeProvider _clock;

    public CalendarApp()
        : this(new LanguageModel(Config.LoadOpenAI()), TimeProvider.System)
    {
    }

    public CalendarApp(ILanguageModel model, TimeProvider clock)
    {
        _translator = new JsonTranslator<CalendarActions>(model);
        _clock = clock;

        // Uncomment to see ALL raw messages to and from the AI
        //base.SubscribeAllEvents(_translator);
    }

    public TypeSchema Schema => _translator.Validator.Schema;

    public override async Task ProcessInputAsync(string input, CancellationToken cancelToken)
    {
        CalendarActions actions = await _translator.TranslateAsync(input,
            [PromptLibrary.Now(_clock.GetLocalNow().DateTime)], cancelToken: cancelToken);
        Console.WriteLine(Microsoft.TypeChat.Json.Stringify(actions));
        PrintUnknown(actions);
    }

    private bool PrintUnknown(CalendarActions calendarActions)
    {
        int countUnknown = 0;
        foreach (var action in calendarActions.Actions)
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
            //Console.WriteLine(app.Schema.Schema.Text);
            await app.RunAsync("📅> ", args.GetOrNull(0));
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
