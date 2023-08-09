﻿// Copyright (c) Microsoft. All rights reserved.

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

    /// <summary>
    /// Call a method with name using the given args
    /// </summary>
    /// <param name="name">method name</param>
    /// <param name="args">arguments for method</param>
    /// <returns>Result, if any</returns>
    public dynamic Call(string name, params dynamic[] args)
    {
        ApiMethod method = _typeInfo[name];
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
