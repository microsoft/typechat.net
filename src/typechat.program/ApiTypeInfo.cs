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
    List<MethodInfo> _methods;
    List<ApiMethod> _typeInfo;

    public ApiTypeInfo(Type type)
        : this(GetPublicMethods(type))
    {
    }
    /// <summary>
    /// Api type information
    /// </summary>
    /// <param name="type">Api type</param>
    /// <param name="apiMethods">Set of methods for your API</param>
    public ApiTypeInfo(MethodInfo[]? apiMethods = null)
    {
        _typeInfo = new List<ApiMethod>();
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
        _typeInfo.Add(new ApiMethod(method));
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
        foreach (var typeInfo in _typeInfo)
        {
            if (typeInfo.Method.Name == name)
            {
                return typeInfo;
            }
        }
        return null;
    }

    static MethodInfo[] GetPublicMethods(Type type, BindingFlags? flags = null)
    {
        flags ??= BindingFlags.Public | BindingFlags.Instance;
        return type.GetMethods(flags.Value);
    }
}
