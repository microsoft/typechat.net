// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// Evaluates a JSON program using a simple interpreter.
/// Function calls in the program are passed to the onCall callback function for validation and dispatch.
/// NOTE: the interpreter is synchronous for simplicity, but will be made entirely async in an upcoming checkin. 
/// </summary>
public class ProgramInterpreter
{
    static readonly dynamic[] EmptyArray = new dynamic[0];

    ApiInvoker _apiInvoker;
    List<dynamic> _results;
    Func<string, dynamic[], dynamic> _handler;

    public ProgramInterpreter(object apiImpl)
    {
        _apiInvoker = new ApiInvoker(apiImpl);
        _handler = _apiInvoker.InvokeMethod;
    }

    public ProgramInterpreter(Func<string, dynamic[], dynamic> handler)
    {
        ArgumentNullException.ThrowIfNull(handler, nameof(handler));
        _handler = handler;
    }

    public dynamic? Run(Program program)
    {
        ArgumentNullException.ThrowIfNull(program, nameof(program));

        _results ??= new List<dynamic>();
        _results.Clear();

        Steps steps = program.Steps;
        for (int i = 0; i < steps.Calls.Length; ++i)
        {
            FunctionCall call = steps.Calls[i];
            dynamic result = Eval(call);
            _results.Add(result);
        }
        return (_results.Count > 0) ? _results[_results.Count - 1] : null;
    }

    dynamic Eval(FunctionCall call)
    {
        dynamic[] args = Eval(call.Args);
        dynamic result = _handler(call.Name, args);
        return result;
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
        dynamic[] results = new dynamic[expr.Value.Length];
        for (int i = 0; i < expr.Value.Length; ++i)
        {
            results[i] = Eval(expr.Value[i]);
        }
        return results;
    }

    JsonObject Eval(ObjectExpr expr)
    {
        return new JsonObject(
            EvalObject(expr)
        );
    }

    IEnumerable<KeyValuePair<string, JsonNode>> EvalObject(ObjectExpr expr)
    {
        foreach (var property in expr.Value)
        {
            dynamic result = Eval(property.Value);
            JsonNode node = result;
            yield return new KeyValuePair<string, JsonNode>(property.Key, node);
        }

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
