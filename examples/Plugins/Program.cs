// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel;
using Microsoft.TypeChat;

namespace Plugins;

public class PluginApp : ConsoleApp
{
    OpenAIConfig _config;
    Kernel _kernel;
    PluginProgramTranslator _programTranslator;
    ProgramInterpreter _interpreter;

    public PluginApp()
    {
        InitPlugins();
        _programTranslator = new PluginProgramTranslator(_kernel, _config.Model);
        _programTranslator.Translator.MaxRepairAttempts = 2;
        _interpreter = new ProgramInterpreter();
        // Uncomment to see ALL raw messages to and from the AI
        // base.SubscribeAllEvents(_programTranslator.Translator);
    }

    public Kernel Kernel => _kernel;
    public string Schema => _programTranslator.Schema;

    public override async Task ProcessInputAsync(string input, CancellationToken cancelToken)
    {
        using Program program = await _programTranslator.TranslateAsync(input, cancelToken);
        program.Print(_programTranslator.Api.TypeName);
        Console.WriteLine();

        if (program.IsComplete)
        {
            await RunProgram(program);
        }
    }

    async Task RunProgram(Program program)
    {
        if (!program.IsComplete)
        {
            return;
        }
        Console.WriteLine("Running program");
        string result = await _interpreter.RunAsync(program, _programTranslator.Api.InvokeAsync);
        if (!string.IsNullOrEmpty(result))
        {
            Console.WriteLine(result);
        }
    }

    void InitPlugins()
    {
        _config = Config.LoadOpenAI();
        _kernel = _config.CreateKernel();
        _kernel.Plugins.AddFromObject(new ShellPlugin());
        _kernel.Plugins.AddFromObject(new FoldersPlugin());
        _kernel.Plugins.AddFromObject(new StringPlugin());
        _kernel.Plugins.AddFromObject(new TimePlugin());
    }

    public static async Task<int> Main(string[] args)
    {
        try
        {
            PluginApp app = new PluginApp();
            // Uncomment to view auto-generated schema
            //Console.WriteLine(app.Schema);

            await app.RunAsync("🤖> ", args.GetOrNull(0));
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
