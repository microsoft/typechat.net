// Copyright (c) Microsoft. All rights reserved.
using System.ComponentModel;
using Microsoft.SemanticKernel.SkillDefinition;

namespace Plugins;

internal class TimePlugin
{
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

}
