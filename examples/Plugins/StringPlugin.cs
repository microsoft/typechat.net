// Copyright (c) Microsoft. All rights reserved.
using System.ComponentModel;
using Microsoft.SemanticKernel.SkillDefinition;

namespace Plugins;

public class StringPlugin
{
    [SKFunction, SKName("concat")]
    [Description("Concat two strings")]
    public string Concat(string x, string y)
    {
        return x + y;
    }

    [SKFunction, SKName("concatWithSep")]
    [Description("Concat two strings using the separator")]
    public string ConcatWithSep(string separator, string x, string y)
    {
        if (!string.IsNullOrEmpty(separator))
        {
            return x + separator + y;
        }
        return x + y;
    }

    [SKFunction, SKName("find")]
    [Description("Returns the lines that contain the given search pattern")]
    public string Find(string lines, string searchPattern)
    {
        List<string> matches = new List<string>();
        string[] splitLines = lines.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        foreach (string line in splitLines)
        {
            if (line.Contains(searchPattern, StringComparison.OrdinalIgnoreCase))
            {
                matches.Add(line);
            }
        }
        return string.Join('\n', matches);
    }
}
