// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel.Orchestration;

namespace Microsoft.TypeChat;

/// <summary>
/// An Api defined by of one or more Semantic Kernel Plugins
/// A Plugin API can be called by Json Programs: thus, the Json Program can write programs
/// that use multiple Kernel plugins
/// </summary>
public class PluginApi
{
    IKernel _kernel;
    string _typeName;
    PluginApiTypeInfo _typeInfo;

    /// <summary>
    /// Create an Api using all registered kernel plugins
    /// </summary>
    /// <param name="kernel"></param>
    public PluginApi(IKernel kernel)
        : this(kernel, "IPluginApi", new PluginApiTypeInfo(kernel))
    {
    }

    /// <summary>
    /// Create an Api with the given Api type name and using the given plugins
    /// </summary>
    /// <param name="kernel"></param>
    /// <param name="typeName"></param>
    /// <param name="typeInfo"></param>
    public PluginApi(IKernel kernel, string typeName, PluginApiTypeInfo typeInfo)
    {
        ArgumentVerify.ThrowIfNull(kernel, nameof(kernel));
        ArgumentVerify.ThrowIfNullOrEmpty(typeName, nameof(typeName));
        ArgumentVerify.ThrowIfNull(typeInfo, nameof(typeInfo));

        _kernel = kernel;
        _typeName = typeName;
        _typeInfo = typeInfo;
    }

    /// <summary>
    /// Api name
    /// </summary>
    public string TypeName => _typeName;

    /// <summary>
    /// Plugins that make up this Api
    /// </summary>
    public PluginApiTypeInfo TypeInfo => _typeInfo;

    /// <summary>
    /// Bind the given function name and args to the plugin that implements the call
    /// </summary>
    /// <param name="name">function name</param>
    /// <param name="args">function args</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public (PluginFunctionName, FunctionView) BindFunction(string name, dynamic[] args)
    {
        var pluginName = PluginFunctionName.Parse(name);
        if (!_typeInfo.TryGetValue(pluginName, out FunctionView function))
        {
            throw new ArgumentException($"Function {name} does not exist");
        }

        return (pluginName, function);
    }

    /// <summary>
    /// Dynamicallyinvoke a plugin. Used by Program Interpreters
    /// </summary>
    /// <param name="name"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public async Task<dynamic> InvokeAsync(string name, dynamic[] args)
    {
        var (functionName, typeInfo) = BindFunction(name, args);
        ISKFunction function = _kernel.GetFunction(functionName);

        IList<ParameterView> parameters = typeInfo.Parameters;
        SKContext context = _kernel.CreateNewContext();
        for (int i = 0; i < args.Length; ++i)
        {
            context.Variables[parameters[i].Name] = args[i].ToString();
        }

        await function.InvokeAsync(context).ConfigureAwait(false);

        return context.Variables.Input;
    }
}
