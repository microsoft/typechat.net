﻿// Copyright (c) Microsoft. All rights reserved.

using System.Reflection;
using System.Text.Json;
using System.Linq.Expressions;
using LinqExpression = System.Linq.Expressions.Expression;

namespace Microsoft.TypeChat.Tests;

public class TestProgram : ProgramTest
{
    //[Fact]
    public void TestSchema()
    {
        TypescriptSchema schema = TypescriptExporter.GenerateSchema(typeof(Program));
        var lines = schema.Schema.Text.Lines();
        Assert.True(lines.Contains("steps", "Call"));
        Assert.True(lines.Contains("func", "string"));
        Assert.True(lines.Contains("args", "Expr[]"));
    }

    [Theory]
    [MemberData(nameof(GetMathPrograms))]
    public void TestParse(string source, double result)
    {
        Program program = new ProgramParser().Parse(source);
        ValidateProgram(program);
    }

    [Fact]
    public void TestParseGeneral()
    {
        var doc = LoadStringPrograms();
        foreach (var obj in doc.RootElement.EnumerateObject())
        {
            var source = obj.Value.GetProperty("source");
            Program program = Json.Parse<Program>(source.ToString());
            ValidateProgram(program);
            program.Dispose();
        }
    }

    [Theory]
    [MemberData(nameof(GetMathPrograms))]
    public void TestJsonConvertor_Math(string source, object result)
    {
        Program program = Json.Parse<Program>(source);
        ValidateProgram(program);
    }

    // This test is basically the Math example turned into a unit test
    // Json Programs wer pregenerated by the AI or written by hand
    [Theory]
    [MemberData(nameof(GetMathPrograms))]
    public void TestInterpreter_Math(string source, double expectedResult)
    {
        Program program = Json.Parse<Program>(source);
        ValidateProgram(program);

        ApiCaller api = new ApiCaller(new APIimpl());
        double result = api.RunProgram(program);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [MemberData(nameof(GetMathPrograms))]
    public async Task TestInterpreter_MathAsync(string source, double expectedResult)
    {
        Program program = Json.Parse<Program>(source);
        ValidateProgram(program);

        ApiCaller api = new ApiCaller(MathAPIAsync.Default);
        double result = (double)await api.RunProgramAsync(program);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [MemberData(nameof(GetStringPrograms))]
    public void TestInterpreter_String(string source, string expectedResult)
    {
        Program program = Json.Parse<Program>(source);
        ValidateProgram(program);

        ApiCaller api = new ApiCaller(new TextApis());
        string result = api.RunProgram(program);
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void TestDynamic()
    {
        dynamic[] args = new dynamic[2];
        args[0] = 3;
        args[1] = 4;
        dynamic result = args[0] + args[1];
        Assert.Equal(7, result);

        APIimpl api = new APIimpl();
        MethodInfo addMethod = GetMethod(api.GetType(), "add");
        result = addMethod.Invoke(api, args);
        Assert.Equal(7, result);
        JsonNode node = result;
        Assert.Equal(7, (double)node);

        args[0] = "Mario";
        Assert.Equal("Mario4", args[0] + args[1]);
        args[1] = "_Minderbinder";
        Assert.Equal("Mario_Minderbinder", args[0] + args[1]);
    }

    [Theory]
    [MemberData(nameof(GetStringPrograms))]
    public void TestProgramValidator_String(string source, string expectedResult)
    {
        Program program = Json.Parse<Program>(source);
        ProgramValidator validator = new ProgramValidator(typeof(IStringAPI));
        validator.Validate(program);
    }

    [Theory]
    [MemberData(nameof(GetMathPrograms))]
    public void TestProgramValidator_Math(string source, string expectedResult)
    {
        Program program = Json.Parse<Program>(source);
        ProgramValidator validator = new ProgramValidator(typeof(IMathAPI));
        validator.Validate(program);
    }

    // TODO: more validation.. actually inspect the AST and compare against
    // the JSON DOM
    void ValidateProgram(Program program)
    {
        Assert.NotNull(program.Steps);

        var calls = program.Steps.Calls;
        Assert.NotNull(calls);
        Assert.True(calls.Length > 0);
        foreach (var call in calls)
        {
            ValidateCall(call);
        }
    }

    [Fact]
    public async Task TestAsync()
    {
        MathAPIAsync mathAsync = MathAPIAsync.Default;
        ApiCaller invoker = new ApiCaller(mathAsync);
        double result = await invoker.CallAsync("add", 4, 5);
        Assert.Equal(9, result);

        APIimpl api = new APIimpl();
        invoker = new ApiCaller(api);
        result = await invoker.CallAsync("add", result, 9);
        Assert.Equal(18, result);
    }

    void ValidateCall(FunctionCall call)
    {
        Assert.NotNull(call.Name);
        Assert.NotEmpty(call.Name);
    }
}
