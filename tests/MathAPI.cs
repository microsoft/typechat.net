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

public class MathAPIAsync : IMathAPIAsync
{
    public MathAPIAsync() { }

    public Task<double> add(double x, double y)
    {
        return Task.FromResult(x + y);
    }

    public Task<double> mul(double x, double y)
    {
        return Task.FromResult(x * y);
    }

    public Task<double> div(double x, double y)
    {
        return Task.FromResult(x / y);
    }

    public Task<double> id(double x)
    {
        return Task.FromResult(x);
    }

    public Task<double> neg(double x)
    {
        return Task.FromResult(-x);
    }

    public Task<double> sub(double x, double y)
    {
        return Task.FromResult(x - y);
    }

    public Task<double> unknown(string text)
    {
        return Task.FromResult(double.NaN);
    }
}
