// Copyright (c) Microsoft. All rights reserved.
using Microsoft.TypeChat;
using Microsoft.TypeChat.Schema;
using Microsoft.TypeChat.SemanticKernel;

namespace Math;

public class Math : ConsoleApp
{
    ProgramTranslator _translator;
    ApiCaller _apiCaller;

    Math()
    {
        _apiCaller = new ApiCaller(new MathAPI());
        _translator = new ProgramTranslator(
            KernelFactory.CreateLanguageModel(Config.LoadOpenAI()),
            TypescriptExporter.GenerateAPI(typeof(IMathAPI))
        );
        _apiCaller.Calling += this.DisplayCall;
        // Uncomment to see ALL raw messages to and from the AI
        // _translator.CompletionReceived += base.OnCompletionReceived;
    }

    public TypeSchema Schema => _translator.Validator.Schema;

    protected override async Task ProcessRequestAsync(string input, CancellationToken cancelToken)
    {
        Program program = await _translator.TranslateAsync(input);
        double result = _apiCaller.RunProgram(program);
        Console.WriteLine($"Result: {result}");
    }

    private void DisplayCall(string functionName, dynamic[] args)
    {
        Console.WriteLine(ApiCaller.CallToString(functionName, args));
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
