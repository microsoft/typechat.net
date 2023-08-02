// Copyright (c) Microsoft. All rights reserved.
using Microsoft.TypeChat;
using Microsoft.TypeChat.SemanticKernel;

namespace Math;

public class Math : ConsoleApp
{
    ProgramTranslator _translator;

    Math()
    {
        string apiDef = File.ReadAllText("mathSchema.ts");
        var languageModel = KernelFactory.CreateLanguageModel(Config.LoadOpenAI());
        _translator = new ProgramTranslator(languageModel, apiDef);
        // Uncomment to see ALL raw messages to and from the AI
        _translator.CompletionReceived += base.OnCompletionReceived;
    }

    public TypeSchema Schema => _translator.Validator.Schema;

    protected override async Task ProcessRequestAsync(string input, CancellationToken cancelToken)
    {
        Program program = await _translator.TranslateAsync(input);
    }

    public static async Task<int> Main(string[] args)
    {
        try
        {
            Math app = new Math();
            // Un-comment to print auto-generated schema at start:
            Console.WriteLine(app.Schema.Schema);
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
