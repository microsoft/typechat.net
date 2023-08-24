// Copyright (c) Microsoft. All rights reserved.
using System.IO;
using System.ComponentModel;
using Microsoft.SemanticKernel.SkillDefinition;

namespace Plugins;

/// <summary>
/// A Semantic Kernel Plugin for "Shell"/Cmd Operations
/// </summary>
public class ShellPlugin
{
    [SKFunction, SKName("getcd")]
    [Description("Get the name of the current directory")]
    public string GetCD()
    {
        return Directory.GetCurrentDirectory();
    }

    [SKFunction, SKName("setcd")]
    [Description("Set the name of the current directory")]
    public void SetCD(
        [Description("Set current directory to this path")]
        string path
    )
    {
        if (!string.IsNullOrEmpty(path))
        {
            Directory.SetCurrentDirectory(path);
        }
    }

    [SKFunction, SKName("output")]
    [Description("Write message to output")]
    public void Output(string message)
    {
        Console.WriteLine(message);
    }

    [SKFunction, SKName("outputLine")]
    [Description("Write message to output followed by new line")]
    public void OutputLine(string message)
    {
        Console.WriteLine(message);
    }

    [SKFunction, SKName("input")]
    [Description("Get user input")]
    public string Input(string? userPrompt)
    {
        if (string.IsNullOrEmpty(userPrompt))
        {
            userPrompt = ">>";
        }
        Console.Write(userPrompt);
        return Console.ReadLine();
    }

    [SKFunction, SKName("readFile")]
    [Description("Read text from a file")]
    public string ReadFile(string filePath)
    {
        return File.ReadAllText(filePath);
    }

    [SKFunction, SKName("writeFile")]
    [Description("Write text to a file")]
    public void WriteFile(string filePath, string text)
    {
        File.WriteAllText(filePath, text);
    }
}
