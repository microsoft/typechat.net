// Copyright (c) Microsoft. All rights reserved.

using Microsoft.TypeChat;

namespace Math;

public class MathApp : ConsoleApp
{
    ProgramTranslator<IMathAPI> _translator;
    Api<IMathAPI> _api;

    public MathApp()
    {
        _api = new MathAPI();
        _translator = new ProgramTranslator<IMathAPI>(
            new LanguageModel(Config.LoadOpenAI()),
            _api
        );
        _api.CallCompleted += this.DisplayCall;
        // Uncomment to see ALL raw messages to and from the AI
        //base.SubscribeAllEvents(_translator);
    }

    public TypeSchema Schema => _translator.Validator.Schema;

    public override async Task ProcessRequestAsync(string input, CancellationToken cancelToken)
    {
        using Program program = await _translator.TranslateAsync(input, cancelToken);

        // Print whatever program was returned
        program.Print(_api.Type.Name);
        Console.WriteLine();

        if (program.IsComplete)
        {
            // IsComplete: If program has steps and program.HasNotTranslated is false
            RunProgram(program);
        }
    }

    void RunProgram(Program program)
    {
        Console.WriteLine("Running program");
        dynamic retval = program.Run(_api);
        if (retval != null && retval is double)
        {
            Console.WriteLine($"Result: {retval}");
        }
        else
        {
            Console.WriteLine("No result");
        }
    }

    void DisplayCall(string functionName, dynamic[] args, dynamic result)
    {
        new ProgramWriter(Console.Out).Write(functionName, args);
        Console.WriteLine($"==> {result}");
    }

    public static async Task<int> Main(string[] args)
    {
        try
        {
            MathApp app = new MathApp();
            // Un-comment to print auto-generated schema at start:
            // Console.WriteLine(app._translator.ApiDef);
            await app.RunAsync("➕➖✖️➗🟰> ", args.GetOrNull(0));
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
