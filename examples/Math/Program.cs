// Copyright (c) Microsoft. All rights reserved.
using System.Diagnostics.CodeAnalysis;
using Microsoft.TypeChat;
using Microsoft.TypeChat.Schema;

namespace Math;

public class Math : ConsoleApp
{
    ProgramTranslator _translator;
    Api<IMathAPI> _api;
    CSharpProgramCompiler _compiler;

    Math()
    {
        _api = new MathAPI();
        _translator = new ProgramTranslator<IMathAPI>(
            new CompletionService(Config.LoadOpenAI()),
            _api
        );
        _compiler = new CSharpProgramCompiler("math");
        _api.CallCompleted += this.DisplayCall;
        // Uncomment to see ALL raw messages to and from the AI
        base.SubscribeAllEvents(_translator);
    }

    public TypeSchema Schema => _translator.Validator.Schema;

    protected override async Task ProcessRequestAsync(string input, CancellationToken cancelToken)
    {
        using Program program = await _translator.TranslateAsync(input);
        DisplayProgram(program);
        Console.WriteLine("Running program");
        dynamic result = program.Run(_api);
        if (result != null && result is double)
        {
            Console.WriteLine($"Result: {result}");
        }
        else
        {
            Console.WriteLine("No result");
        }
    }

    private void DisplayProgram(Program program)
    {
        using StringWriter writer = new StringWriter();
        new ProgramWriter(writer).Write(program, typeof(IMathAPI));

        string code = writer.ToString();

        Console.WriteLine(code);

        string diagnostics = _compiler.GetDiagnostics(code, "./MathApi.cs");
        Console.WriteLine(diagnostics);
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
