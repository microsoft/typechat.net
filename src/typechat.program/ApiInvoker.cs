// Copyright (c) Microsoft. All rights reserved.

using System.Reflection;

namespace Microsoft.TypeChat;

/// <summary>
/// Uses Reflection to call an API
/// T must be an interface
/// </summary>
public class ApiInvoker
{
    public static readonly object?[] EmptyArgs = Array.Empty<object?>();

    ApiTypeInfo _typeInfo;
    object _apiImpl;

    public ApiInvoker(object apiImpl)
        : this(new ApiTypeInfo(apiImpl.GetType()), apiImpl)
    {
    }

    public ApiInvoker(ApiTypeInfo typeInfo, object apiImpl)
    {
        ArgumentNullException.ThrowIfNull(typeInfo, nameof(typeInfo));
        ArgumentNullException.ThrowIfNull(apiImpl, nameof(apiImpl));
        _typeInfo = typeInfo;
        _apiImpl = apiImpl;
    }

    public AnyJsonValue InvokeMethod(string name, AnyJsonValue[] args)
    {
        ApiMethod method = _typeInfo[name];
        object?[] callArgs = CreateCallArgs(name, args, method.Params);
        object? retVal = method.Method.Invoke(_apiImpl, callArgs);
        return AnyJsonValue.FromObject(method.ReturnType.ParameterType, retVal);
    }

    // Future: caching, pooling
    object?[] CreateCallArgs(string name, AnyJsonValue[] jsonArgs, ParameterInfo[] paramInfo)
    {
        if (jsonArgs.Length != paramInfo.Length)
        {
            ProgramException.ThrowArgCountMismatch(name, paramInfo.Length, jsonArgs.Length);
        }
        if (paramInfo.Length == 0)
        {
            return EmptyArgs;
        }
        object?[] args = new object[jsonArgs.Length];
        for (int i = 0; i < paramInfo.Length; ++i)
        {
            Type paramType = paramInfo[i].ParameterType;
            args[i] = jsonArgs[i].ToObject(paramType);
        }
        return args;
    }
}
