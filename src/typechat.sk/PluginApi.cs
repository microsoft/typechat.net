// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel.Orchestration;

namespace Microsoft.TypeChat;

public class PluginApi
{
    IKernel _kernel;
    string _typeName;
    PluginApiTypeInfo _typeInfo;

    public PluginApi(IKernel kernel)
        : this(kernel, "IPluginApi", new PluginApiTypeInfo(kernel))
    {

    }
    public PluginApi(IKernel kernel, string typeName, PluginApiTypeInfo typeInfo)
    {
        ArgumentNullException.ThrowIfNull(kernel, nameof(kernel));
        ArgumentNullException.ThrowIfNullOrEmpty(typeName, nameof(typeName));
        ArgumentNullException.ThrowIfNull(typeInfo, nameof(typeInfo));

        _kernel = kernel;
        _typeName = typeName;
        _typeInfo = typeInfo;
    }

    public string TypeName => _typeName;
    public PluginApiTypeInfo TypeInfo => _typeInfo;

    public (PluginFunctionName, FunctionView) BindFunction(string name, dynamic[] args)
    {
        var pluginName = PluginFunctionName.Parse(name);
        if (!_typeInfo.TryGetValue(pluginName, out FunctionView function))
        {
            throw new ArgumentException($"Function {name} does not exist");
        }
        int argCount = (args != null) ? args.Length : 0;
        int paramCount = (function.Parameters != null) ? function.Parameters.Count : 0;
        if (argCount != paramCount)
        {
            throw new ArgumentException($"Function {name}: Arg Count mismatch. Expected {paramCount}, Got {argCount}");
        }
        return (pluginName, function);
    }

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
        await function.InvokeAsync(context);
        return context.Variables.Input;
    }
}
