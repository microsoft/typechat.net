// Copyright (c) Microsoft. All rights reserved.

using Microsoft.TypeChat;
using Microsoft.TypeChat.Schema;

namespace Math;

[Comment("This is a schema for writing programs that evaluate expressions")]
public interface IMathAPI
{
    [Comment("Add two numbers")]
    double add(double x, double y);
    [Comment("Subtract two numbers")]
    double sub(double x, double y);
    [Comment("Multiply two numbers")]
    double mul(double x, double y);
    [Comment("Divide two numbers")]
    double div(double x, double y);
    [Comment("Identity function")]
    double id(double x);
    [Comment("Negate a number")]
    double neg(double x);
    [Comment("Unknown request")]
    double unknown(string text);
}

/// <summary>
/// Any implementation of the Math API
/// </summary>
public class MathAPI : IMathAPI
{
    public MathAPI() { }

    public double add(double x, double y) => x + y;

    public double sub(double x, double y) => x - y;

    public double mul(double x, double y) => x * y;

    public double div(double x, double y) => x / y;

    public double id(double x) => x;

    public double neg(double x) => -x;

    public double unknown(string text) => double.NaN;

    public AnyJsonValue HandleCall(string name, AnyJsonValue[] args)
    {
        ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));
        switch (name)
        {
            default:
                return double.NaN;
            case "add":
                return BinaryOp(add, name, args);
            case "sub":
                return BinaryOp(sub, name, args);
            case "mul":
                return BinaryOp(mul, name, args);
            case "div":
                return BinaryOp(div, name, args);
            case "neg":
                return UnaryOp(neg, name, args);
            case "id":
                return UnaryOp(id, name, args);
        }
    }

    AnyJsonValue UnaryOp(Func<double, double> fn, string name, AnyJsonValue[] args)
    {
        CheckArgLength(name, 1, args);
        return fn(args[0]);
    }

    AnyJsonValue BinaryOp(Func<double, double, double> fn, string name, AnyJsonValue[] args)
    {
        CheckArgLength(name, 2, args);
        return fn(args[0], args[1]);
    }

    void CheckArgLength(string fnName, int expectedLength, AnyJsonValue[] args)
    {
        if (args.Length != args.Length)
        {
            throw new ProgramException(ProgramException.ErrorCode.ArgCountMismatch, $"{fnName}: Expected {expectedLength}, Got {args.Length}");
        }
    }
}
