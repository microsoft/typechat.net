// Copyright (c) Microsoft. All rights reserved.
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.TypeChat;
using Microsoft.TypeChat.Schema;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Skills.Core;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;
using System.ComponentModel;

namespace Plugins;

public class PluginApp : ConsoleApp
{
    IKernel _kernel;
    ProgramTranslator _translator;
    PluginApi _pluginApi;
    string _pluginSchema;
    ProgramInterpreter _interpreter;

    public PluginApp()
    {
        InitPlugins();
        _translator = new ProgramTranslator(
            new CompletionService(Config.LoadOpenAI()),
            new ProgramValidator(new PluginProgramValidator(_pluginApi.TypeInfo)),
            _pluginSchema
        );
        _translator.MaxRepairAttempts = 2;
        _interpreter = new ProgramInterpreter();
    }

    public IKernel Kernel => _kernel;
    public string Schema => _pluginSchema;

    protected override async Task ProcessRequestAsync(string input, CancellationToken cancelToken)
    {
        Result<Program> program = await _translator.TranslateAsync(input);
        if (!program.Success)
        {
            Console.WriteLine($"## Failed:\n{program.Message}");
        }
        PrintProgram(program, program.Success);
        if (program.Success)
        {
            await RunProgram(program);
        }
    }

    void PrintProgram(Program program, bool success)
    {
        if (program == null) { return; }

        if (!program.PrintNotTranslated())
        {
            program.PrintTranslationNotes();
        }

        if (program.HasSteps)
        {
            if (!program.IsComplete || !success)
            {
                Console.WriteLine("Possible program with possibly needed APIs:");
            }
            new ProgramWriter(Console.Out).Write(program, typeof(object));
            Console.WriteLine();
        }
    }

    async Task RunProgram(Program program)
    {
        if (!program.IsComplete)
        {
            return;
        }
        string result = await _interpreter.RunAsync(program, _pluginApi.InvokeAsync);
        if (!string.IsNullOrEmpty(result))
        {
            Console.WriteLine(result);
        }
    }

    void InitPlugins()
    {
        _kernel = Config.LoadOpenAI().CreateKernel();
        _kernel.ImportSkill(new ShellPlugin());
        _kernel.ImportSkill(new StringPlugin());
        _kernel.ImportSkill(new TimePlugin());

        _pluginApi = new PluginApi(_kernel);
        _pluginSchema = _pluginApi.TypeInfo.ExportSchema("IPluginApi");
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
