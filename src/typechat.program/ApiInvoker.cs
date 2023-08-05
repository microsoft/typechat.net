// Copyright (c) Microsoft. All rights reserved.

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
    Dictionary<string, object?[]> _argsPool;

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
        _argsPool = new Dictionary<string, object?[]>();
    }

    public AnyJsonValue InvokeMethod(string name, AnyJsonValue[] args)
    {
        ApiMethod method = _typeInfo[name];
        object?[] callArgs = CreateCallArgs(name, args, method.Params);
        object? retVal = method.Method.Invoke(_apiImpl, callArgs);
        return AnyJsonValue.FromObject(method.ReturnType.ParameterType, retVal);
    }

    // Future: caching, pooling
    object?[] CreateCallArgs(string name, AnyJsonValue[] jsonArgs, ParameterInfo[] paramsInfo)
    {
        if (jsonArgs.Length != paramsInfo.Length)
        {
            ProgramException.ThrowArgCountMismatch(name, paramsInfo.Length, jsonArgs.Length);
        }
        if (paramsInfo.Length == 0)
        {
            return EmptyArgs;
        }
        //object?[] args = new object[jsonArgs.Length];
        object?[] args = GetArgs(name, paramsInfo.Length);
        for (int i = 0; i < paramsInfo.Length; ++i)
        {
            Type paramType = paramsInfo[i].ParameterType;
            args[i] = jsonArgs[i].ToObject(paramType);
        }
        return args;
    }

    object?[] GetArgs(string name, int argLength)
    {
        object?[] args = _argsPool.GetValueOrDefault(name);
        if (args == null)
        {
            args = new object?[argLength];
            _argsPool[name] = args;
        }
        Debug.Assert(args.Length == argLength);
        return args;
    }
}
