// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

internal static class MetadataEx
{

    internal static bool IsGlobal(this KernelFunctionMetadata function)
    {
        return (string.IsNullOrEmpty(function.PluginName) || "_GLOBAL_FUNCTIONS_" == function.PluginName);
    }

    internal static bool HasDescription(this KernelParameterMetadata param)
    {
        return !string.IsNullOrEmpty(param.Description);
    }

    internal static KernelFunction GetFunction(this Kernel kernel, PluginFunctionName plugin)
    {
        KernelFunction function;
        if (plugin.IsGlobal)
        {
            function = kernel.Plugins.GetFunction(null, plugin.FunctionName);
        }
        else
        {
            function = kernel.Plugins.GetFunction(plugin.PluginName, plugin.FunctionName);
        }
        return function;
    }

    internal static PluginFunctionName ToPluginName(this KernelFunctionMetadata function)
    {
        /*
        if (function.IsGlobal())
        {
            return new PluginFunctionName(function.Name);
        }
        return new PluginFunctionName(function.PluginName, function.Name);
        */
        return new PluginFunctionName(function.Name);
    }

    internal static bool IsNullable(this KernelParameterMetadata param)
    {
        return (param.DefaultValue is not null);
    }
}
