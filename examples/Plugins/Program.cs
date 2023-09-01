// Copyright (c) Microsoft. All rights reserved.
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.TypeChat;
using Microsoft.TypeChat.Schema;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.AI.TextCompletion;

namespace Plugins;

public class PluginApp : ConsoleApp
{
    OpenAIConfig _config;
    IKernel _kernel;
    ProgramTranslator _translator;
    PluginApi _pluginApi;
    string _pluginSchema;
    ProgramInterpreter _interpreter;

    public PluginApp()
    {
        InitPlugins();
        _translator = new ProgramTranslator(
            _kernel.LanguageModel(_config.Model),
            new ProgramValidator(new PluginProgramValidator(_pluginApi.TypeInfo)),
            _pluginSchema
        );
        _translator.MaxRepairAttempts = 2;
        _interpreter = new ProgramInterpreter();
        // Uncomment to see ALL raw messages to and from the AI
        //base.SubscribeAllEvents(_translator);
    }

    public IKernel Kernel => _kernel;
    public string Schema => _pluginSchema;

    public override async Task ProcessRequestAsync(string input, CancellationToken cancelToken)
    {
        using Program program = await _translator.TranslateAsync(input, cancelToken);
        program.Print(_pluginApi.TypeName);
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
        string result = await _interpreter.RunAsync(program, _pluginApi.InvokeAsync);
        if (!string.IsNullOrEmpty(result))
        {
            Console.WriteLine(result);
        }
    }

    void InitPlugins()
    {
        _config = Config.LoadOpenAI();
        _kernel = _config.CreateKernel();
        _kernel.ImportSkill(new ShellPlugin());
        _kernel.ImportSkill(new StringPlugin());
        _kernel.ImportSkill(new TimePlugin());

        _pluginApi = new PluginApi(_kernel);
        _pluginSchema = _pluginApi.TypeInfo.ExportSchema(_pluginApi.TypeName);
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
            return -1;
        }

        return 0;
    }
}
