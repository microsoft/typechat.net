// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

internal static class MetadataEx
{

    internal static bool IsGlobal(this FunctionView fview)
    {
        return ("_GLOBAL_FUNCTIONS_" == fview.SkillName);
    }

    internal static bool HasDescription(this ParameterView param)
    {
        return !string.IsNullOrEmpty(param.Description);
    }

    internal static ISKFunction GetFunction(this IKernel kernel, PluginFunctionName plugin)
    {
        ISKFunction function;
        if (plugin.IsGlobal)
        {
            function = kernel.Skills.GetFunction(plugin.FunctionName);
        }
        else
        {
            function = kernel.Skills.GetFunction(plugin.PluginName, plugin.FunctionName);
        }
        return function;
    }

    internal static PluginFunctionName ToPluginName(this FunctionView fview)
    {
        // Temporary hack to make pretty printing possible
        if (fview.IsGlobal())
        {
            return new PluginFunctionName(fview.Name);
        }
        return new PluginFunctionName(fview.SkillName, fview.Name);
    }

    internal static bool IsNullable(this ParameterView param)
    {
        return !string.IsNullOrEmpty(param.DefaultValue);
    }
}
