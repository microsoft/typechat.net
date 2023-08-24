// Copyright (c) Microsoft. All rights reserved.
using System.ComponentModel;
using Microsoft.SemanticKernel.SkillDefinition;

namespace Plugins;

public class StringPlugin
{
    [SKFunction, SKName("concat")]
    [Description("Concat TWO strings")]
    public string Concat(string x, string y)
    {
        return x + y;
    }

    [SKFunction, SKName("concat_sep")]
    [Description("Concat TWO strings with the given separator")]
    public string ConcatSep(string separator, string x, string y)
    {
        if (!string.IsNullOrEmpty(separator))
        {
            return x + separator + y;
        }
        return x + y;
    }

    [SKFunction, SKName("uppercase")]
    [Description("Convert string to uppercase")]
    public string Uppercase(string input)
    {
        return input.ToUpper();
    }


    [SKFunction, SKName("lowercase")]
    [Description("Convert string to lowercase")]
    public string Lowercase(string input)
    {
        return input.ToLower();
    }
}
