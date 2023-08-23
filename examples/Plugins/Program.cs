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
    PluginApiTypeInfo _typeInfo;
    string _pluginSchema;
    //ProgramInterpreter _interpreter;

    public PluginApp()
    {
        InitPlugins();
        _translator = new ProgramTranslator(
            new CompletionService(Config.LoadOpenAI()),
            new ProgramValidator(new PluginProgramValidator(_typeInfo)),
            _pluginSchema
        );
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
        if (program != null)
        {
            PrintProgram(program, program.Success);
        }
    }

    void PrintProgram(Program program, bool success)
    {
        if (program.HasNotTranslated)
        {
            Console.WriteLine("I could not translate the following:");
            WriteLines(program.NotTranslated);
            Console.WriteLine();
        }
        else if (!string.IsNullOrEmpty(program.TranslationNotes))
        {
            Console.WriteLine("Translation Notes:");
            Console.WriteLine(program.TranslationNotes);
            Console.WriteLine();
        }

        if (program.HasSteps)
        {
            if (!program.IsValid || !success)
            {
                Console.WriteLine("Possible program with needed APIs:");
            }
            new ProgramWriter(Console.Out).Write(program, typeof(object));
        }
    }

    void InitPlugins()
    {
        _kernel = Config.LoadOpenAI().CreateKernel();
        _kernel.ImportSkill(new FileIOSkill());
        //_kernel.ImportSkill(new FallbackSkill(), "Fallback");
        _typeInfo = new PluginApiTypeInfo(_kernel);
        _pluginSchema = _typeInfo.ExportSchema(
            "IPluginApi"
            //"When a user request cannot be satisfied, use the Fallback methods"
            );
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

public class FallbackSkill
{
    [SKFunction]
    [Description("User requests that you cannot handle")]
    public Task<string> NotHandledAsync(string text)
    {
        return Task.FromResult(string.Empty);
    }
}
