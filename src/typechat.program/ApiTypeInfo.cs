// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public struct ApiMethod
{
    public ApiMethod(MethodInfo method)
    {
        ArgumentNullException.ThrowIfNull(method, nameof(method));
        Method = method;
        Params = method.GetParameters();
        ReturnType = method.ReturnParameter;
    }

    public MethodInfo Method { get; private set; }
    public ParameterInfo[] Params { get; private set; }
    public ParameterInfo ReturnType { get; private set; }
}

public class ApiTypeInfo
{
    Dictionary<string, ApiMethod> _typeInfo;

    public ApiTypeInfo(Type type)
        : this(GetPublicMethods(type))
    {
    }

    public ApiTypeInfo(MethodInfo[]? apiMethods = null)
    {
        _typeInfo = new Dictionary<string, ApiMethod>();
        if (apiMethods != null)
        {
            Add(apiMethods);
        }
    }

    public ApiMethod this[string functionName]
    {
        get
        {
            ApiMethod? method = Get(functionName);
            if (method == null)
            {
                ProgramException.ThrowFunctionNotFound(functionName);
            }
            return method.Value;
        }
    }

    public void Add(MethodInfo method)
    {
        _typeInfo.Add(method.Name, new ApiMethod(method));
    }

    public void Add(MethodInfo[] methods)
    {
        foreach (var method in methods)
        {
            Add(method);
        }
    }

    public ApiMethod? Get(string name)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(name, nameof(name));
        if (_typeInfo.TryGetValue(name, out ApiMethod method))
        {
            return method;
        }
        return null;
    }

    static MethodInfo[] GetPublicMethods(Type type, BindingFlags? flags = null)
    {
        flags ??= BindingFlags.Public | BindingFlags.Instance;
        return type.GetMethods(flags.Value);
    }
}
