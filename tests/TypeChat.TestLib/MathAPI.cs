// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Tests;

public class MathAPI : IMathAPI
{
    public static double SmallPi = 3.142;

    public static MathAPI Default = new MathAPI();

    public double add(double x, double y) => x + y;

    public double sub(double x, double y) => x - y;

    public double mul(double x, double y) => x * y;

    public double div(double x, double y) => x / y;

    public double neg(double x) => -x;

    public double pi() => MathAPI.SmallPi;

    public double power(double x, double power) => Math.Pow(x, power);

    public double sqrRoot(double x) => Math.Sqrt(x);

    public double addVector(double[] vector)
    {
        double sum = 0.0;
        for (int i = 0; i < vector.Length; ++i)
        {
            sum += vector[i];
        }
        return sum;
    }
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

    public Task<double> pi() => SlowResult(MathAPI.SmallPi);

    public Task<double> power(double x, double power) => SlowResult(Math.Pow(x, power));

    public Task<double> sqrRoot(double x) => SlowResult(Math.Sqrt(x));

    public Task<double> addVector(double[] vector)
    {
        double sum = 0.0;
        for (int i = 0; i < vector.Length; ++i)
        {
            sum += vector[i];
        }
        return SlowResult(sum);
    }

    private async Task<double> SlowResult(double result)
    {
        await Task.Delay(500).ConfigureAwait(false);
        return result;
    }
}
