// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Tests;

public class MathAPI
{
    public enum Operators
    {
        Add,
        Sub,
        Mul,
        Div,
        Neg,
        Id
    }

    public AnyJsonValue HandleCall(string name, AnyJsonValue[] args)
    {
        ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));
        switch (name)
        {
            default:
                return double.NaN;
            case "add":
                return BinaryOp(name, Operators.Add, args);
            case "sub":
                return BinaryOp(name, Operators.Sub, args);
            case "mul":
                return BinaryOp(name, Operators.Mul, args);
            case "div":
                return BinaryOp(name, Operators.Div, args);
            case "neg":
                return UnaryOp(name, Operators.Neg, args);
            case "id":
                return UnaryOp(name, Operators.Id, args);
        }
    }

    AnyJsonValue UnaryOp(string name, Operators op, AnyJsonValue[] args)
    {
        CheckArgLength(name, 1, args);
        switch(op)
        {
            default:
                throw new NotSupportedException();
            case Operators.Neg:
                double value = args[0];
                return -value;
            case Operators.Id:
                return args[0];
        }
    }

    AnyJsonValue BinaryOp(string name, Operators op, AnyJsonValue[] args)
    {
        CheckArgLength(name, 2, args);
        double x = args[0];
        double y = args[1];
        switch (op)
        {
            default:
                throw new NotSupportedException();
            case Operators.Add:
                return x + y;
            case Operators.Sub:
                return x - y;
            case Operators.Mul:
                return x * y;
            case Operators.Div:
                return x / y;
        }
    }

    void CheckArgLength(string fnName, int expectedLength, AnyJsonValue[] args)
    {
        if (args.Length != args.Length)
        {
            throw new ProgramException(ProgramException.ErrorCode.InvalidArgCount, $"{fnName}: Expected {expectedLength}, Got {args.Length}");
        }
    }
}
