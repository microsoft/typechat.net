// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// Type information for a method in an Api
/// Type information is from System.Reflection but held onto for performance
/// </summary>
public class ApiMethod
{
    /// <summary>
    /// Create the type information
    /// </summary>
    /// <param name="method"></param>
    public ApiMethod(MethodInfo method)
    {
        ArgumentVerify.ThrowIfNull(method, nameof(method));
        Method = method;
        Params = method.GetParameters();
        ReturnType = method.ReturnParameter;
    }

    /// <summary>
    /// Api method
    /// </summary>
    public MethodInfo Method { get; private set; }

    /// <summary>
    /// Method parameters
    /// </summary>
    public ParameterInfo[] Params { get; private set; }

    /// <summary>
    /// Method return type
    /// </summary>
    public ParameterInfo ReturnType { get; private set; }
}

/// <summary>
/// Type information for an Api
/// </summary>
public class ApiTypeInfo
{
    private List<ApiMethod> _typeInfo;

    /// <summary>
    /// Create an Api using all public methods of the given type
    /// </summary>
    /// <param name="type">type</param>
    public ApiTypeInfo(Type type)
        : this(GetPublicMethods(type))
    {
    }

    /// <summary>
    /// Api type information
    /// </summary>
    /// <param name="apiMethods">Set of methods for your API</param>
    public ApiTypeInfo(MethodInfo[]? apiMethods = null)
    {
        _typeInfo = new List<ApiMethod>();
        if (apiMethods is not null)
        {
            Add(apiMethods);
        }
    }

    /// <summary>
    /// Binds the function name to the method in the Api
    /// If not found, throws a ProgramException
    /// </summary>
    /// <param name="methodName"></param>
    /// <returns></returns>
    public ApiMethod this[string methodName]
    {
        get
        {
            ApiMethod method = Get(methodName);
            if (method is null)
            {
                ProgramException.ThrowFunctionNotFound(methodName);
            }
            return method;
        }
    }

    /// <summary>
    /// Add a method to the Api
    /// </summary>
    /// <param name="method"></param>
    public void Add(MethodInfo method)
    {
        _typeInfo.Add(new ApiMethod(method));
    }

    /// <summary>
    /// Add multiple methods to the Api
    /// </summary>
    /// <param name="methods"></param>
    public void Add(MethodInfo[] methods)
    {
        ArgumentVerify.ThrowIfNull(methods, nameof(methods));

        foreach (var method in methods)
        {
            Add(method);
        }
    }

    /// <summary>
    /// Try to bind the method name. If not found, returns null
    /// </summary>
    /// <param name="methodName"></param>
    /// <returns></returns>
    public ApiMethod? Get(string methodName)
    {
        ArgumentVerify.ThrowIfNullOrEmpty(methodName, nameof(methodName));

        foreach (var typeInfo in _typeInfo)
        {
            if (typeInfo.Method.Name == methodName)
            {
                return typeInfo;
            }
        }
        return null;
    }

    public bool HasAsyncMethods()
    {
        foreach (var typeInfo in _typeInfo)
        {
            if (typeInfo.ReturnType.IsAsync())
            {
                return true;
            }
        }
        return false;
    }

    private static MethodInfo[] GetPublicMethods(Type type, BindingFlags? flags = null)
    {
        flags ??= BindingFlags.Public | BindingFlags.Instance;
        return type.GetMethods(flags.Value);
    }
}
