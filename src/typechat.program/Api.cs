// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// Represents an API.
/// - Type information for the Api.
/// - Ability to call the API in a late bound dynamic fashion
/// </summary>
public class Api
{
    static readonly object?[] s_emptyArgs = Array.Empty<object?>();

    ApiTypeInfo _typeInfo;
    object _apiImpl;

    /// <summary>
    /// Create an Api using ALL Public instance methods of the supplied apiImpl
    /// </summary>
    /// <param name="apiImpl">object that implements t`he API</param>
    public Api(object apiImpl)
        : this(new ApiTypeInfo(apiImpl.GetType()), apiImpl)
    {

    }

    /// <summary>
    /// Create an Api using the supplied methods + type info implemented in the given object instance
    /// </summary>
    /// <param name="typeInfo">Type information for the Api</param>
    /// <param name="apiImpl">object instance that implements the API</param>
    public Api(ApiTypeInfo typeInfo, object apiImpl)
    {
        ArgumentVerify.ThrowIfNull(typeInfo, nameof(typeInfo));
        ArgumentVerify.ThrowIfNull(apiImpl, nameof(apiImpl));
        _typeInfo = typeInfo;
        _apiImpl = apiImpl;
    }

    /// <summary>
    /// Type information for this Api
    /// </summary>
    public ApiTypeInfo TypeInfo => _typeInfo;

    /// <summary>
    /// The object that implements the Api
    /// </summary>
    public object Implementation => _apiImpl;

    /// <summary>
    /// Diagnostics
    /// </summary>
    public event Action<string, dynamic[], dynamic> CallCompleted;

    /// <summary>
    /// Call a method with name using the given args
    /// A ProgramInterpreter can use this method to call APIs
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
    /// A ProgramInterpreter can use this method to call APIs
    /// </summary>
    /// <param name="name">method name</param>
    /// <param name="args">args</param>
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

    dynamic[] CreateCallArgs(string name, dynamic[] jsonArgs, ParameterInfo[] paramsInfo)
    {
        if (jsonArgs.Length != paramsInfo.Length)
        {
            ProgramException.ThrowArgCountMismatch(name, jsonArgs.Length, paramsInfo.Length);
        }
        if (paramsInfo.Length == 0)
        {
            return s_emptyArgs;
        }
        // If any of input parameters are JsonObjects, deserialize them
        ConvertObjects(jsonArgs, paramsInfo);
        return jsonArgs;
    }

    /// <summary>
    /// Dynamically type cast/convert args to the expected type
    /// </summary>
    dynamic[] ConvertObjects(dynamic[] jsonArgs, ParameterInfo[] paramsInfo)
    {
        for (int i = 0; i < jsonArgs.Length; ++i)
        {
            jsonArgs[i] = ConvertObject(jsonArgs[i], paramsInfo[i].ParameterType);
        }
        return jsonArgs;
    }

    dynamic[] ConvertObjects(dynamic[] jsonArgs, Type expectedType)
    {
        for (int i = 0; i < jsonArgs.Length; ++i)
        {
            jsonArgs[i] = ConvertObject(jsonArgs[i], expectedType);
        }
        return jsonArgs;
    }

    dynamic ConvertObject(dynamic arg, Type expectedType)
    {
        Type argType = arg.GetType();
        if (argType == expectedType)
        {
            return arg;
        }
        if (arg is JsonObject jsonObj)
        {
            if (expectedType != typeof(JsonObject))
            {
                return JsonSerializer.Deserialize(jsonObj, expectedType);
            }
            return arg;
        }
        if (expectedType.IsArray != argType.IsArray)
        {
            // Won't try to convert arrays to scalars and vice versa. Let Reflection throw an error
            return arg;
        }
        if (!expectedType.IsArray)
        {
            // Convert plain old scalar if we need to
            return Convert.ChangeType(arg, expectedType);
        }

        expectedType = expectedType.GetElementType();
        if (argType.GetElementType() == expectedType)
        {
            // No conversion needed
            return arg;
        }
        return ConvertArray(arg as Array, expectedType);
    }

    Array ConvertArray(Array array, Type expectedType)
    {
        Array convertedArray = Array.CreateInstance(expectedType, array.Length);
        for (int i = 0; i < array.Length; ++i)
        {
            dynamic value = ConvertObject(array.GetValue(i), expectedType);
            convertedArray.SetValue(value, i);
        }
        return convertedArray;
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

public class Api<T> : Api
{
    public Api(object apiImpl)
        : base(new ApiTypeInfo(typeof(T)), apiImpl)
    {
    }

    public Type Type => typeof(T);

    public TypeSchema GenerateSchema()
    {
        return TypescriptExporter.GenerateAPI(Type);
    }

    public static implicit operator Api<T>(T apiImpl)
    {
        return new Api<T>(apiImpl);
    }
}
