// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Tests;

public class MathAPI : IMathAPI
{
    public static MathAPI Default = new MathAPI();

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
    public static MathAPIAsync Default = new MathAPIAsync();

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

public class PersonAPI : IPersonApi
{
    public static PersonAPI Default = new PersonAPI();
    public static Api Caller = new Api(Default);

    public Person makePerson(Name name, int age)
    {
        return new Person { Name = name, Age = age };
    }
    public bool isPerson(Person person, Name name, int age)
    {
        return (person.Name.FirstName == name.FirstName &&
               person.Name.LastName == name.LastName &&
               person.Age == age);
    }
    public string toJson(Person person)
    {
        return Json.Stringify(person);
    }
}
