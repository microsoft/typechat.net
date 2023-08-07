// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Tests;

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
