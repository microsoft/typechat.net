// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel;
using Microsoft.TypeChat;

namespace Plugins;

public class PluginApp : ConsoleApp
{
    private OpenAIConfig _config;
    private IKernel _kernel;
    private PluginProgramTranslator _programTranslator;
    private ProgramInterpreter _interpreter;

    public PluginApp()
    {
        InitPlugins();
        _programTranslator = new PluginProgramTranslator(_kernel, _config.Model);
        _programTranslator.Translator.MaxRepairAttempts = 2;
        _interpreter = new ProgramInterpreter();
        // Uncomment to see ALL raw messages to and from the AI
        //base.SubscribeAllEvents(_translator.Translator);
    }

    public IKernel Kernel => _kernel;
    public string Schema => _programTranslator.Schema;

    public override async Task ProcessInputAsync(string input, CancellationToken cancelToken = default)
    {
        using Program program = await _programTranslator.TranslateAsync(input, cancelToken);
        program.Print(_programTranslator.Api.TypeName);
        Console.WriteLine();

        if (program.IsComplete)
        {
            await RunProgramAsync(program, cancelToken);
        }
    }

    private async Task RunProgramAsync(Program program, CancellationToken cancelToken)
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

    private void InitPlugins()
    {
        _config = Config.LoadOpenAI();
        _kernel = _config.CreateKernel();
        _kernel.ImportSkill(new ShellPlugin());
        _kernel.ImportSkill(new FoldersPlugin());
        _kernel.ImportSkill(new StringPlugin());
        _kernel.ImportSkill(new TimePlugin());
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
