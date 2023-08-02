// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public class ProgramInterpreter
{
    List<AnyValue> _results;

    public ProgramInterpreter()
    {
        _results = new List<AnyValue>();
    }

    public AnyValue Run(Program program, Func<Call, AnyValue, AnyValue> caller)
    {
        ArgumentNullException.ThrowIfNull(program, nameof(program));
        _results.Clear();

        Steps steps = program.Steps;
        for (int i = 0; i < steps.Calls.Length; ++i)
        {
            Call call = steps.Calls[i];
            AnyValue args = Eval(call.Args);
            AnyValue result = caller(call, args);
            _results.Add(result);
        }
        return (_results.Count > 0) ? _results[_results.Count - 1] : AnyValue.Undefined;
    }

    AnyValue Eval(Expr expr)
    {
        switch(expr)
        {
            default:
                break;

            case ValueExpr value:
                return Eval(value);

            case ArrayExpr array:
                return Eval(array);
        }
        return AnyValue.Undefined;
    }

    AnyValue Eval(Expr[] expressions)
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

    AnyValue Eval(ArrayExpr expr)
    {
        AnyValue[] results = new AnyValue[expr.Value.Length];
        for (int i = 0; i < expr.Value.Length; ++i)
        {
            results[i] = Eval(expr.Value[i]);
        }
        return results;
    }
}
