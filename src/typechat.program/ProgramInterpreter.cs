// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public class ProgramInterpreter
{
    List<AnyValue> _results;
    Func<string, AnyValue[], AnyValue> _handler;

    public ProgramInterpreter(Func<string, AnyValue[], AnyValue> handler)
    {
        ArgumentNullException.ThrowIfNull(handler, nameof(handler));
        _results = new List<AnyValue>();
        _handler = handler;
    }

    public AnyValue Run(Program program)
    {
        ArgumentNullException.ThrowIfNull(program, nameof(program));
        _results.Clear();

        Steps steps = program.Steps;
        for (int i = 0; i < steps.Calls.Length; ++i)
        {
            Call call = steps.Calls[i];
            AnyValue result = Eval(call);
            _results.Add(result);
        }
        return (_results.Count > 0) ? _results[_results.Count - 1] : AnyValue.Undefined;
    }

    AnyValue Eval(Call call)
    {
        AnyValue[] args = Eval(call.Args);
        AnyValue result = _handler(call.Name, args);
        return result;
    }

    AnyValue Eval(Expr expr)
    {
        switch(expr)
        {
            default:
                break;

            case Call call:
                return Eval(call);

            case ResultRef result:
                return Eval(result);

            case ValueExpr value:
                return Eval(value);

            case ArrayExpr array:
                return Eval(array);
        }
        return AnyValue.Undefined;
    }

    AnyValue[] Eval(Expr[] expressions)
    {
        if (expressions.Length == 0)
        {
            return AnyValue.EmptyArray;
        }

        AnyValue[] args = new AnyValue[expressions.Length];
        for (int i = 0; i < expressions.Length; ++i)
        {
            args[i] = Eval(expressions[i]);
        }
        return args;
    }


    AnyValue Eval(ValueExpr expr)
    {
        switch(expr.Value.ValueKind)
        {
            default:
                throw new ProgramException(ProgramException.ErrorCode.UnsupportedType, $"{expr.Value.ValueKind}");
            case JsonValueKind.String:
                return expr.Value.GetString();
            case JsonValueKind.Number:
                return expr.Value.GetDouble();
        }
    }

    AnyValue[] Eval(ArrayExpr expr)
    {
        AnyValue[] results = new AnyValue[expr.Value.Length];
        for (int i = 0; i < expr.Value.Length; ++i)
        {
            results[i] = Eval(expr.Value[i]);
        }
        return results;
    }

    AnyValue Eval(ResultRef expr)
    {
        if (expr.Ref >= _results.Count)
        {
            throw new ProgramException(ProgramException.ErrorCode.NoResult, $"Referencing {expr.Ref} from {_results.Count} results");
        }
        return _results[expr.Ref];
    }
}
