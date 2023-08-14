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
        _api = new Api<IMathAPI>(new MathAPI());
        _translator = new ProgramTranslator(new CompletionService(Config.LoadOpenAI()), _api.Type);
        _api.CallCompleted += this.DisplayCall;
        // Uncomment to see ALL raw messages to and from the AI
        // _translator.CompletionReceived += base.OnCompletionReceived;
    }

    public TypeSchema Schema => _translator.Validator.Schema;

    protected override async Task ProcessRequestAsync(string input, CancellationToken cancelToken)
    {
        Program program = await _translator.TranslateAsync(input);
        double result = program.Run(_api);
        Console.WriteLine($"Result: {result}");
    }

    private void DisplayCall(string functionName, dynamic[] args, dynamic result)
    {
        Console.Write(Api.CallToString(functionName, args));
        Console.Write(" => ");
        Console.WriteLine(result);
    }

    public static async Task<int> Main(string[] args)
    {
        try
        {
            Math app = new Math();
            // Un-comment to print auto-generated schema at start:
            //Console.WriteLine(app.Schema.Schema);
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
