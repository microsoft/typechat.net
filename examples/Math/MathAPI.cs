// Copyright (c) Microsoft. All rights reserved.

using Microsoft.TypeChat;

namespace Math;

[Comment("Schema for writing programs that evaluate Math expressions")]
public interface IMathAPI
{
    [Comment("x + y")]
    double add(double x, double y);
    [Comment("x - y")]
    double sub(double x, double y);
    [Comment("x multiplied by y")]
    double mul(double x, double y);
    [Comment("x divided by y")]
    double div(double x, double y);
    [Comment("x modulo y")]
    double mod(double x, double y);
    [Comment("min of two numbers")]
    double min(double x, double y);
    [Comment("max of two numbers")]
    double max(double x, double y);
    [Comment("-x")]
    double neg(double x);
    [Comment("id function. Return x")]
    double id(double x);
    [Comment("Square root")]
    double sqrt(double x);
    [Comment("x raised to y")]
    double power(double x, double y);
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

    public double neg(double x) => -x;

    public double id(double x) => x;

    public double mod(double x, double y) => x % y;

    public double min(double x, double y) => System.Math.Min(x, y);

    public double max(double x, double y) => System.Math.Max(x, y);

    public double sqrt(double x) => System.Math.Sqrt(x);

    public double power(double x, double y) => System.Math.Pow(x, y);
}
