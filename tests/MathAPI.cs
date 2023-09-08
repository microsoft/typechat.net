// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Tests;

public class MathAPI : IMathAPI
{
    public static MathAPI Default = new MathAPI();

    public double add(double x, double y) => x + y;

    public double sub(double x, double y) => x - y;

    public double mul(double x, double y) => x * y;

    public double div(double x, double y) => x / y;

    public double neg(double x) => -x;
}

public class MathAPIAsync : IMathAPIAsync
{
    public static MathAPIAsync Default = new MathAPIAsync();

    public MathAPIAsync() { }

    public async Task<double> add(double x, double y)
    {
        return await SlowResult(x + y);
    }

    public async Task<double> mul(double x, double y)
    {
        return await SlowResult(x * y);
    }

    public Task<double> div(double x, double y)
    {
        return SlowResult(x * y);
    }

    public Task<double> id(double x)
    {
        return SlowResult(x);
    }

    public Task<double> neg(double x)
    {
        return SlowResult(-x);
    }

    public Task<double> sub(double x, double y)
    {
        return SlowResult(x - y);
    }

    public Task<double> unknown(string text)
    {
        return SlowResult(double.NaN);
    }

    async Task<double> SlowResult(double result)
    {
        await Task.Delay(500).ConfigureAwait(false);
        return result;
    }
}
