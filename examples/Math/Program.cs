// Copyright (c) Microsoft. All rights reserved.
using Microsoft.TypeChat;
using Microsoft.TypeChat.Schema;

namespace Math;

public class Math : ConsoleApp
{
    ProgramTranslator _translator;
    Api<IMathAPI> _api;

    Math()
    {
        _api = new MathAPI();
        _translator = new ProgramTranslator<IMathAPI>(
            new CompletionService(Config.LoadOpenAI()),
            _api
        );
        _api.CallCompleted += this.DisplayCall;
        // Uncomment to see ALL raw messages to and from the AI
        //base.SubscribeAllEvents(_translator);
    }

    public TypeSchema Schema => _translator.Validator.Schema;

    protected override async Task ProcessRequestAsync(string input, CancellationToken cancelToken)
    {
        using Program program = await _translator.TranslateAsync(input);
        DisplayProgram(program);
        Console.WriteLine("Running program");
        double result = program.Run(_api);
        Console.WriteLine($"Result: {result}");
    }

    private void DisplayProgram(Program program)
    {
        new ProgramWriter(Console.Out).Write(program, typeof(IMathAPI));
    }

    private void DisplayCall(string functionName, dynamic[] args, dynamic result)
    {
        new ProgramWriter(Console.Out).Write(functionName, args);
        Console.WriteLine($"==> {result}");
    }

    public static async Task<int> Main(string[] args)
    {
        try
        {
            Math app = new Math();
            // Un-comment to print auto-generated schema at start:
            // Console.WriteLine(app._translator.ApiDef);
            await app.RunAsync("➕➖✖️➗🟰> ", args.GetOrNull(0));
        }
        catch (Exception ex)
        {
            WriteError(ex);
            return -1;
        }

        return 0;
    }
}
