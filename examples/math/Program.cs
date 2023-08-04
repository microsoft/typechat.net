// Copyright (c) Microsoft. All rights reserved.
using Microsoft.TypeChat;
using Microsoft.TypeChat.Schema;
using Microsoft.TypeChat.SemanticKernel;

namespace Math;

public class Math : ConsoleApp
{
    ProgramTranslator _translator;
    ProgramInterpreter _interpreter;

    Math()
    {
        var apiSchema = TypescriptExporter.GenerateSchema(typeof(IMathAPI));
        var languageModel = KernelFactory.CreateLanguageModel(Config.LoadOpenAI());
        _translator = new ProgramTranslator(languageModel, apiSchema.Schema.Text);
        _interpreter = new ProgramInterpreter(HandleCall);
        // Uncomment to see ALL raw messages to and from the AI
        // _translator.CompletionReceived += base.OnCompletionReceived;
    }

    public TypeSchema Schema => _translator.Validator.Schema;

    protected override async Task ProcessRequestAsync(string input, CancellationToken cancelToken)
    {
        Program program = await _translator.TranslateAsync(input);
        double result = _interpreter.Run(program);
        Console.WriteLine(result);
    }

    AnyJsonValue HandleCall(string name, AnyJsonValue[] args)
    {
        switch(name)
        {
            default:
                return BinaryOp(name, args);
            case "unknown":
                return double.NaN;
            case "neg":
                return -args[0];
            case "id":
                return args[0];
        }
    }

    double BinaryOp(string name, AnyJsonValue[] args)
    {
        if (args.Length < 2)
        {
            throw new InvalidOperationException();
        }
        double x = args[0];
        double y = args[1];
        switch (name)
        {
            default:
                return double.NaN;
            case "add":
                return x + y;
            case "sub":
                return x - y;
            case "mul":
                return x * y;
            case "div":
                return x / y;
        }
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
            Console.WriteLine(ex);
            return -1;
        }

        return 0;
    }
}
