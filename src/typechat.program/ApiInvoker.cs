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

    public dynamic InvokeMethod(string name, dynamic[] args)
    {
        ApiMethod method = _typeInfo[name];
        dynamic[] callArgs = CreateCallArgs(name, args, method.Params);
        dynamic retVal = method.Method.Invoke(_apiImpl, callArgs);
        return retVal;
    }

    dynamic[] CreateCallArgs(string name, dynamic[] jsonArgs, ParameterInfo[] paramsInfo)
    {
        if (jsonArgs.Length != paramsInfo.Length)
        {
            return CreateCallArgsArray(name, jsonArgs, paramsInfo);
        }
        if (paramsInfo.Length == 0)
        {
            return EmptyArgs;
        }
        return jsonArgs;
    }

    dynamic[] CreateCallArgsArray(string name, dynamic[] jsonArgs, ParameterInfo[] paramsInfo)
    {
        Debug.Assert(paramsInfo.Length == 1);
        if (!paramsInfo[0].ParameterType.IsArray)
        {
            ProgramException.ThrowArgCountMismatch(name, paramsInfo.Length, jsonArgs.Length);
        }
        // Future: Pool these
        dynamic[] args = new dynamic[1];
        args[0] = jsonArgs;
        return args;
    }
}
