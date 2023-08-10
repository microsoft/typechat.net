// Copyright (c) Microsoft. All rights reserved.

using System.Text;

namespace Microsoft.TypeChat;

/// <summary>
/// Runs programs against an API
/// Relies on the DLR for type checking etc. 
/// </summary>
public class ApiCaller
{
    public static readonly object?[] EmptyArgs = Array.Empty<object?>();

    ApiTypeInfo _typeInfo;
    object _apiImpl;
    ProgramInterpreter _interpreter;

    public ApiCaller(object apiImpl)
        : this(new ApiTypeInfo(apiImpl.GetType()), apiImpl)
    {
    }

    public ApiCaller(ApiTypeInfo typeInfo, object apiImpl)
    {
        ArgumentNullException.ThrowIfNull(typeInfo, nameof(typeInfo));
        ArgumentNullException.ThrowIfNull(apiImpl, nameof(apiImpl));
        _typeInfo = typeInfo;
        _apiImpl = apiImpl;
        _interpreter = new ProgramInterpreter();
    }

    public ApiTypeInfo TypeInfo => _typeInfo;

    public event Action<string, dynamic[]> Calling;

    /// <summary>
    /// Call a method with name using the given args
    /// </summary>
    /// <param name="name">method name</param>
    /// <param name="args">arguments for method</param>
    /// <returns>Result, if any</returns>
    public dynamic Call(string name, params dynamic[] args)
    {
        ApiMethod method = _typeInfo[name];

        NotifyCalling(name, args);
        dynamic[] callArgs = CreateCallArgs(name, args, method.Params);
        dynamic retVal = method.Method.Invoke(_apiImpl, callArgs);
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
        ApiMethod method = _typeInfo[name];
        if (!method.ReturnType.IsAsync())
        {
            return Call(name, args);
        }

        NotifyCalling(name, args);

        dynamic[] callArgs = CreateCallArgs(name, args, method.Params);
        dynamic task = (Task)method.Method.Invoke(_apiImpl, callArgs);
        return await task;
    }

    /// <summary>
    /// Run a program that targets this API
    /// </summary>
    /// <param name="program">Json program to run</param>
    /// <returns>Program result</returns>
    public dynamic RunProgram(Program program)
    {
        ArgumentNullException.ThrowIfNull(program, nameof(program));
        return _interpreter.Run(program, Call);
    }

    /// <summary>
    /// Run a program against this API asynchronously
    /// </summary>
    /// <param name="program"></param>
    /// <returns></returns>
    public Task<dynamic> RunProgramAsync(Program program)
    {
        return _interpreter.RunAsync(program, CallAsync);
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
        // If any of input paramters are JsonObjects, deserialize them
        ConvertJsonObjects(jsonArgs, paramsInfo);

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

    dynamic[] ConvertJsonObjects(dynamic[] jsonArgs, ParameterInfo[] paramsInfo)
    {
        Type jsonObjType = typeof(JsonObject);
        for (int i = 0; i < jsonArgs.Length; ++i)
        {
            JsonObject jsonObj = jsonArgs[i] as JsonObject;
            if (jsonObj != null && paramsInfo[i].ParameterType != jsonObjType)
            {
                object typedObj = jsonObj.Deserialize(paramsInfo[i].ParameterType);
                jsonArgs[i] = typedObj;
            }
        }
        return jsonArgs;
    }

    void NotifyCalling(string name, dynamic[] args)
    {
        if (Calling != null)
        {
            try
            {
                Calling(name, args);
            }
            catch { }
        }
    }
}
