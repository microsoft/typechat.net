// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// An Api defined by of one or more Semantic Kernel Plugins
/// A Plugin API can be called by Json Programs: thus, the Json Program can write programs
/// that use multiple Kernel plugins
/// </summary>
public class PluginApi
{
    private readonly Kernel _kernel;
    private readonly string _typeName;
    private readonly PluginApiTypeInfo _typeInfo;

    /// <summary>
    /// Create an Api using all registered kernel plugins
    /// </summary>
    /// <param name="kernel"></param>
    public PluginApi(Kernel kernel)
        : this(kernel, "IPluginApi", new PluginApiTypeInfo(kernel))
    {
    }

    /// <summary>
    /// Create an Api with the given Api type name and using the given plugins
    /// </summary>
    /// <param name="kernel"></param>
    /// <param name="typeName"></param>
    /// <param name="typeInfo"></param>
    public PluginApi(Kernel kernel, string typeName, PluginApiTypeInfo typeInfo)
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
    public (PluginFunctionName, KernelFunctionMetadata) BindFunction(string name, dynamic[] args)
    {
        var pluginName = PluginFunctionName.Parse(name);
        if (!_typeInfo.TryGetValue(pluginName, out KernelFunctionMetadata function))
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
    /// <remarks>Uses .ConfigureAndAwait(false) since this is a library and potentially blocking call.</remarks>
    public async Task<dynamic> InvokeAsync(string name, dynamic[] args)
    {
        var (functionName, typeInfo) = BindFunction(name, args);
        KernelFunction function = _kernel.GetFunction(functionName);

        var parameters = typeInfo.Parameters;
        KernelArguments kernelArgs = new KernelArguments();
        for (int i = 0; i < args.Length; ++i)
        {
            kernelArgs[parameters[i].Name] = args[i];
        }
        var result = await function.InvokeAsync(_kernel, kernelArgs).ConfigureAwait(false);
        return result.GetValue<dynamic>();
    }
}
