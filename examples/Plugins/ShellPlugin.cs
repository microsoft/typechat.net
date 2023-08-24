// Copyright (c) Microsoft. All rights reserved.
using System.IO;
using System.ComponentModel;
using Microsoft.SemanticKernel.SkillDefinition;

namespace Plugins;

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

    [SKFunction, SKName("date")]
    [Description("Get the current date")]
    public string Date()
    {
        return DateTime.Now.ToShortDateString();
    }

    [SKFunction, SKName("time")]
    [Description("Get the current date")]
    public string Time()
    {
        return DateTime.Now.ToShortTimeString();
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

    [SKFunction, SKName("concat")]
    [Description("Concat two strings")]
    public string Concat(string x, string y)
    {
        return x + y;
    }
}
