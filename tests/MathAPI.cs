// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Tests;

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

public class MathAPI : IMathAPI
{
    public double add(double x, double y) => x + y;

    public double sub(double x, double y) => x - y;

    public double mul(double x, double y) => x * y;

    public double div(double x, double y) => x / y;

    public double id(double x) => x;

    public double neg(double x) => -x;

    public double unknown(string text) => double.NaN;
}
