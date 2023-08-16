// Copyright (c) Microsoft. All rights reserved.
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.TypeChat;
using Microsoft.TypeChat.Schema;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Skills.Core;
using Microsoft.SemanticKernel.Orchestration;

namespace Plugins;

public class PluginApp : ConsoleApp
{
    IKernel _kernel;
    ProgramTranslator _translator;
    string _pluginSchema;
    //ProgramInterpreter _interpreter;

    public PluginApp()
    {
        InitSkills();
        _translator = new ProgramTranslator(
            new CompletionService(Config.LoadOpenAI()),
            _pluginSchema
        );
    }

    public IKernel Kernel => _kernel;
    public string Schema => _pluginSchema;

    protected override async Task ProcessRequestAsync(string input, CancellationToken cancelToken)
    {
        Program program = await _translator.TranslateAsync(input);
        new ProgramWriter(Console.Out).Write(program, typeof(object));
    }

    void InitSkills()
    {
        _kernel = Config.LoadOpenAI().CreateKernel();
        _kernel.ImportSkill(new HttpSkill());
        _kernel.ImportSkill(new FileIOSkill());
        _kernel.ImportSkill(new TimeSkill());
        _pluginSchema = PluginTypescriptExporter.ExportRegistered(_kernel);
    }

    public static async Task<int> Main(string[] args)
    {
        try
        {
            PluginApp app = new PluginApp();
            Console.WriteLine(app.Schema);

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
