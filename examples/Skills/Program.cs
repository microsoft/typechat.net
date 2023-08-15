// Copyright (c) Microsoft. All rights reserved.
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.TypeChat;
using Microsoft.TypeChat.Schema;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.SkillDefinition;
using Microsoft.SemanticKernel.Skills.Core;
using Microsoft.TypeChat.SemanticKernel;

namespace Skills;

public class SkillsApp : ConsoleApp
{
    IKernel _kernel;

    public SkillsApp()
    {
        InitKernel();
    }

    public IKernel Kernel => _kernel;

    protected override async Task ProcessRequestAsync(string input, CancellationToken cancelToken)
    {
    }

    void InitKernel()
    {
        _kernel = Config.LoadOpenAI().CreateKernel();
        _kernel.ImportSkill(new HttpSkill());
        _kernel.ImportSkill(new FileIOSkill());
    }

    public string ExportSkillMetadata()
    {
        using StringWriter writer = new StringWriter();
        SkillTypescriptExporter exporter = new SkillTypescriptExporter(writer);

        var skills = _kernel.Skills.GetFunctionsView();
        exporter.Export("ISkills", skills.NativeFunctions.Values);

        return writer.ToString();
    }

    public static async Task<int> Main(string[] args)
    {
        try
        {
            SkillsApp app = new SkillsApp();
            Console.WriteLine(app.ExportSkillMetadata());

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

