// Copyright (c) Microsoft. All rights reserved.
using System.IO;
using System.ComponentModel;
using Microsoft.SemanticKernel.SkillDefinition;
using Microsoft.SemanticKernel.AI.TextCompletion;
using Microsoft.TypeChat;

namespace Plugins;

/// <summary>
/// A Semantic Kernel Plugin for "Shell"/Cmd Operations
/// </summary>
public class ShellPlugin
{
    [SKFunction, SKName("getEnv")]
    [Description("Get environment variable")]
    public string GetEnv(string name)
    {
        return Environment.GetEnvironmentVariable(name);
    }

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
        if (!string.IsNullOrEmpty(path) &&
            Directory.Exists(path))
        {
            Directory.SetCurrentDirectory(path);
        }
    }

    [SKFunction, SKName("md")]
    [Description("Make a directory if it does not exist")]
    public void MD(string path)
    {
        if (!File.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    [SKFunction, SKName("dir")]
    [Description("List files and sub-directories, each item on its own line")]
    public string Dir(
        [Description("directory path")]
        string path,
        [Description("true to list recursively - default should be false")]
        bool recursive = false)
    {
        if (Directory.Exists(path))
        {
            EnumerationOptions enumOptions = new EnumerationOptions { RecurseSubdirectories = recursive };
            var directories = Directory.EnumerateDirectories(path, "*", enumOptions);
            var files = Directory.EnumerateFiles(path, "*", enumOptions);
            return string.Join('\n', directories.Concat(files));
        }

        return string.Empty;
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
        if (!string.IsNullOrEmpty(userPrompt))
        {
            Console.Write(userPrompt);
        }
        Console.Write(">>");
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
