// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Tests;

[Comment("This is a schema for writing programs that evaluate Math expressions")]
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

public interface IMathAPIAsync
{
    [Comment("Add two numbers")]
    Task<double> add(double x, double y);
    [Comment("Subtract two numbers")]
    Task<double> sub(double x, double y);
    [Comment("Multiply two numbers")]
    Task<double> mul(double x, double y);
    [Comment("Divide two numbers")]
    Task<double> div(double x, double y);
    [Comment("Identity function")]
    Task<double> id(double x);
    [Comment("Negate a number")]
    Task<double> neg(double x);
    [Comment("Unknown request")]
    Task<double> unknown(string text);
}

[Comment("This is an API for String Operations")]
public interface IStringAPI
{
    [Comment("Concat all arguments")]
    string concat(dynamic[] args);
    [Comment("Uppercase the text")]
    string uppercase(string text);
    [Comment("Lowercase text")]
    string lowercase(string text);
}

[Comment("API to get the current date and time")]
public interface ITimeAPI
{
    [Comment("Return current date and time")]
    string dateTime();
    [Comment("Return current date")]
    string date();
    [Comment("Return current time")]
    string time();
}

[Comment("API to manipulate a Person object")]
public interface IPersonApi
{
    Person makePerson(Name name, int age);
    string toJson(Person person);
    string toJsonArray(Person[] persons);
    bool isPerson(Person person, Name name, int age);
}
