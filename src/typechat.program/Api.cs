// Copyright (c) Microsoft. All rights reserved.

using System.Text;

namespace Microsoft.TypeChat;

public class Api
{
    public static readonly object?[] EmptyArgs = Array.Empty<object?>();

    ApiTypeInfo _typeInfo;
    object _apiImpl;
    ProgramInterpreter _interpreter;

    public Api(object apiImpl)
        : this(new ApiTypeInfo(apiImpl.GetType()), apiImpl)
    {

    }
    public Api(ApiTypeInfo typeInfo, object apiImpl)
    {
        ArgumentNullException.ThrowIfNull(typeInfo, nameof(typeInfo));
        ArgumentNullException.ThrowIfNull(apiImpl, nameof(apiImpl));
        _typeInfo = typeInfo;
        _apiImpl = apiImpl;
        _interpreter = new ProgramInterpreter();
    }

    public ApiTypeInfo TypeInfo => _typeInfo;

    public event Action<string, dynamic[], dynamic> CallCompleted;

    /// <summary>
    /// Call a method with name using the given args
    /// </summary>
    /// <param name="name">method name</param>
    /// <param name="args">arguments for method</param>
    /// <returns>Result, if any</returns>
    public dynamic Call(string name, params dynamic[] args)
    {
        var method = BindMethod(name, args);
        dynamic[] callArgs = CreateCallArgs(name, args, method.Params);
        dynamic retVal = method.Method.Invoke(_apiImpl, callArgs);
        NotifyCall(name, args, retVal);
        return retVal;
    }

    /// <summary>
    /// Call a method with name using the given args
    /// </summary>
    /// <param name="name"></param>
    /// <param name="args"></param>
    /// <returns>Result, if any</returns>
    public async Task<dynamic> CallAsync(string name, params dynamic[] args)
    {
        ApiMethod method = BindMethod(name, args);
        if (!method.ReturnType.IsAsync())
        {
            return Call(name, args);
        }

        dynamic[] callArgs = CreateCallArgs(name, args, method.Params);
        dynamic task = (Task)method.Method.Invoke(_apiImpl, callArgs);
        var result = await task;
        NotifyCall(name, args, result);
        return result;
    }

    public static string CallToString(string functionName, dynamic[] args)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append($"{functionName}(");
        for (int i = 0; i < args.Length; ++i)
        {
            if (i > 0) { sb.Append(", "); }
            sb.Append(args[i]);
        }
        sb.Append(")");
        return sb.ToString();
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
        // If any of input paramters are JsonObjects, deserialize them
        ConvertJsonObjects(jsonArgs, paramsInfo);
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

    dynamic[] ConvertJsonObjects(dynamic[] jsonArgs, ParameterInfo[] paramsInfo)
    {
        Type jsonObjType = typeof(JsonObject);
        for (int i = 0; i < jsonArgs.Length; ++i)
        {
            JsonObject jsonObj = jsonArgs[i] as JsonObject;
            if (jsonObj != null && paramsInfo[i].ParameterType != jsonObjType)
            {
                object typedObj = JsonSerializer.Deserialize(jsonObj, paramsInfo[i].ParameterType);
                jsonArgs[i] = typedObj;
            }
        }
        return jsonArgs;
    }

    void NotifyCall(string name, dynamic[] args, dynamic result)
    {
        if (CallCompleted != null)
        {
            try
            {
                CallCompleted(name, args, result);
            }
            catch { }
        }
    }

    protected virtual ApiMethod BindMethod(string name, dynamic[] args)
    {
        return _typeInfo[name];
    }

}
/// <summary>
/// Runs programs against an API
/// Relies on the DLR for type checking etc. 
/// </summary>
public class Api<T> : Api
{
    public Api(object apiImpl)
        : base(new ApiTypeInfo(typeof(T)), apiImpl)
    {
    }

    public Type Type => typeof(T);
}
