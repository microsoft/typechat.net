// Copyright (c) Microsoft. All rights reserved.
using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace Plugins;

internal class TimePlugin
{
    [KernelFunction("date")]
    [Description("Get the current date")]
    public string Date()
    {
        return DateTime.Now.ToShortDateString();
    }

    [KernelFunction("time")]
    [Description("Get the current date")]
    public string Time()
    {
        return DateTime.Now.ToShortTimeString();
    }

}
