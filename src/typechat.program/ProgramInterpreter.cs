// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// Evaluates a JSON program using a simple interpreter.
/// Function calls in the program are passed to the onCall callback function for validation and dispatch.
/// NOTE: the interpreter is synchronous for simplicity, but will be made entirely async in an upcoming checkin. 
/// </summary>
public class ProgramInterpreter
{
    List<AnyJsonValue> _results;
    Func<string, AnyJsonValue[], AnyJsonValue> _handler;

    public ProgramInterpreter(Func<string, AnyJsonValue[], AnyJsonValue> handler)
    {
        ArgumentNullException.ThrowIfNull(handler, nameof(handler));
        _results = new List<AnyJsonValue>();
        _handler = handler;
    }

    public AnyJsonValue Run(Program program)
    {
        ArgumentNullException.ThrowIfNull(program, nameof(program));
        _results.Clear();

        Steps steps = program.Steps;
        for (int i = 0; i < steps.Calls.Length; ++i)
        {
            FunctionCall call = steps.Calls[i];
            AnyJsonValue result = Eval(call);
            _results.Add(result);
        }
        return (_results.Count > 0) ? _results[_results.Count - 1] : AnyJsonValue.Undefined;
    }

    AnyJsonValue Eval(FunctionCall call)
    {
        AnyJsonValue[] args = Eval(call.Args);
        AnyJsonValue result = _handler(call.Name, args);
        return result;
    }

    AnyJsonValue Eval(Expression expr)
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

        return AnyJsonValue.Undefined;
    }

    AnyJsonValue[] Eval(Expression[] expressions)
    {
        if (expressions.Length == 0)
        {
            return AnyJsonValue.EmptyArray;
        }

        AnyJsonValue[] args = new AnyJsonValue[expressions.Length];
        for (int i = 0; i < expressions.Length; ++i)
        {
            args[i] = Eval(expressions[i]);
        }
        return args;
    }


    AnyJsonValue Eval(ValueExpr expr)
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

    AnyJsonValue[] Eval(ArrayExpr expr)
    {
        AnyJsonValue[] results = new AnyJsonValue[expr.Value.Length];
        for (int i = 0; i < expr.Value.Length; ++i)
        {
            results[i] = Eval(expr.Value[i]);
        }
        return results;
    }

    Dictionary<string, AnyJsonValue> Eval(ObjectExpr expr)
    {
        Dictionary<string, AnyJsonValue> results = new Dictionary<string, AnyJsonValue>(expr.Value.Count);
        foreach (var property in expr.Value)
        {
            results[property.Key] = Eval(property.Value);
        }
        return results;
    }

    AnyJsonValue Eval(ResultReference expr)
    {
        if (expr.Ref >= _results.Count)
        {
            ProgramException.ThrowInvalidResultRef(expr.Ref, _results.Count);
        }
        return _results[expr.Ref];
    }
}
