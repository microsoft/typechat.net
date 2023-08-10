// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// Evaluates a JSON program using a simple lighweight interpreter.
/// This class is stateful, and not thread safe For multiple threads, use multiple interpreters: interpreters are extremely lightweight
/// Function calls in the program are passed to the handler callback function for dispatch.
/// </summary>
public class ProgramInterpreter
{
    static readonly dynamic[] EmptyArray = new dynamic[0];

    List<dynamic> _results;
    Func<string, dynamic[], dynamic>? _callHandler;
    Func<string, dynamic[], Task<dynamic>>? _callHandlerAsync;

    public ProgramInterpreter()
    {
        _results = new List<dynamic>();
    }

    public dynamic? Run(Program program, Func<string, dynamic[], dynamic> callHandler)
    {
        ArgumentNullException.ThrowIfNull(program, nameof(program));
        ArgumentNullException.ThrowIfNull(callHandler, nameof(callHandler));

        Clear();

        _callHandler = callHandler;
        Steps steps = program.Steps;
        for (int i = 0; i < steps.Calls.Length; ++i)
        {
            FunctionCall call = steps.Calls[i];
            dynamic result = Eval(call);
            _results.Add(result);
        }
        return GetResult();
    }

    public async Task<dynamic?> RunAsync(Program program, Func<string, dynamic[], Task<dynamic>> callHandlerAsync)
    {
        ArgumentNullException.ThrowIfNull(program, nameof(program));
        ArgumentNullException.ThrowIfNull(callHandlerAsync, nameof(callHandlerAsync));

        Clear();
        _callHandlerAsync = callHandlerAsync;

        Steps steps = program.Steps;
        for (int i = 0; i < steps.Calls.Length; ++i)
        {
            FunctionCall call = steps.Calls[i];
            dynamic result = await EvalAsync(call);
            _results.Add(result);
        }
        return GetResult();
    }

    void Clear()
    {
        _results.Clear();
        _callHandler = null;
        _callHandlerAsync = null;
    }

    dynamic? GetResult()
    {
        dynamic? result = (_results.Count > 0) ? _results[_results.Count - 1] : null;
        return result;
    }

    dynamic Eval(FunctionCall call)
    {
        dynamic[] args = Eval(call.Args);
        dynamic result = _callHandler(call.Name, args);
        return result;
    }

    async Task<dynamic> EvalAsync(FunctionCall call)
    {
        dynamic[] args = await EvalAsync(call.Args);
        return await _callHandlerAsync(call.Name, args);
    }

    dynamic Eval(Expression expr)
    {
        switch (expr)
        {
            default:
                break;

            case FunctionCall call:
                return Eval(call);

            case ResultReference result:
                return Eval(result);

            case ValueExpr value:
                return Eval(value);

            case ArrayExpr array:
                return Eval(array);

            case ObjectExpr obj:
                return Eval(obj);
        }

        return null;
    }

    async Task<dynamic> EvalAsync(Expression expr)
    {
        switch (expr)
        {
            default:
                break;

            case FunctionCall call:
                return await EvalAsync(call);

            case ResultReference result:
                return Eval(result);

            case ValueExpr value:
                return Eval(value);

            case ArrayExpr array:
                return await EvalAsync(array);

            case ObjectExpr obj:
                return await EvalAsync(obj);
        }

        return null;
    }

    dynamic[] Eval(Expression[] expressions)
    {
        if (expressions.Length == 0)
        {
            return EmptyArray;
        }

        dynamic[] args = new dynamic[expressions.Length];
        for (int i = 0; i < expressions.Length; ++i)
        {
            args[i] = Eval(expressions[i]);
        }
        return args;
    }

    async Task<dynamic[]> EvalAsync(Expression[] expressions)
    {
        if (expressions.Length == 0)
        {
            return EmptyArray;
        }

        dynamic[] args = new dynamic[expressions.Length];
        for (int i = 0; i < expressions.Length; ++i)
        {
            args[i] = await EvalAsync(expressions[i]);
        }
        return args;
    }

    dynamic Eval(ValueExpr expr)
    {
        switch (expr.Value.ValueKind)
        {
            default:
                throw new ProgramException(ProgramException.ErrorCode.TypeNotSupported, $"{expr.Value.ValueKind}");
            case JsonValueKind.String:
                return expr.Value.GetString();
            case JsonValueKind.Number:
                return expr.Value.GetDouble();
        }
    }

    dynamic[] Eval(ArrayExpr expr)
    {
        return Eval(expr.Value);
    }

    Task<dynamic[]> EvalAsync(ArrayExpr expr)
    {
        return EvalAsync(expr.Value);
    }

    JsonObject Eval(ObjectExpr expr)
    {
        JsonObject jsonObject = new JsonObject();
        foreach (var property in expr.Value)
        {
            dynamic result = Eval(property.Value);
            JsonNode node = result;
            jsonObject.Add(property.Key, node);
        }
        return jsonObject;
    }

    async Task<JsonObject> EvalAsync(ObjectExpr expr)
    {
        JsonObject jsonObj = new JsonObject();
        foreach (var property in expr.Value)
        {
            dynamic result = await EvalAsync(property.Value);
            JsonNode node = result;
            jsonObj.Add(property.Key, node);
        }
        return jsonObj;
    }

    dynamic Eval(ResultReference expr)
    {
        if (expr.Ref >= _results.Count)
        {
            ProgramException.ThrowInvalidResultRef(expr.Ref, _results.Count);
        }
        return _results[expr.Ref];
    }
}
