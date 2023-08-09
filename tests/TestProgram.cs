﻿// Copyright (c) Microsoft. All rights reserved.

using System.Reflection;
using System.Text.Json;

namespace Microsoft.TypeChat.Tests;

public class TestProgram : TypeChatTest
{
    static JsonDocument LoadMathPrograms()
    {
        string json = File.ReadAllText("mathPrograms.json");
        return Json.Parse<JsonDocument>(json);
    }

    static JsonDocument LoadStringPrograms()
    {
        string json = File.ReadAllText("stringPrograms.json");
        return Json.Parse<JsonDocument>(json);
    }

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

        ApiInvoker api = new ApiInvoker(new MathAPI());
        ProgramInterpreter interpreter = new ProgramInterpreter(api.InvokeMethod);
        double result = interpreter.Run(program);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [MemberData(nameof(GetStringPrograms))]
    public void TestInterpreter_String(string source, string expectedResult)
    {
        Program program = Json.Parse<Program>(source);
        ValidateProgram(program);

        TextApis api = new TextApis();
        ProgramInterpreter interpreter = new ProgramInterpreter(api);
        string result = interpreter.Run(program);
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

        MathAPI api = new MathAPI();
        MethodInfo addMethod = GetMethod(api.GetType(), "add");
        result = addMethod.Invoke(api, args);
        Assert.Equal(7, result);
        JsonNode node = result;
        Assert.Equal(7, (double) node);

        args[0] = "Toby";
        Assert.Equal("Toby4", args[0] + args[1]);
        args[1] = "_McDuff";
        Assert.Equal("Toby_McDuff", args[0] + args[1]);
    }

    [Theory]
    [MemberData(nameof(GetStringPrograms))]
    public void TestProgramValidator_String(string source, string expectedResult)
    {
        Program program = Json.Parse<Program>(source);
        ValidateProgram(program);

        ProgramValidator validator = new ProgramValidator(typeof(IStringAPI));
        validator.Validate(program.Steps);
    }

    [Theory]
    [MemberData(nameof(GetMathPrograms))]
    public void TestProgramValidator_Math(string source, string expectedResult)
    {
        Program program = Json.Parse<Program>(source);
        ValidateProgram(program);

        ProgramValidator validator = new ProgramValidator(typeof(IMathAPI));
        validator.Validate(program.Steps);
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
        MathAPIAsync mathAsync = new MathAPIAsync();
        ApiInvoker invoker = new ApiInvoker(mathAsync);
        double result = await invoker.InvokeMethodAsync("add", 4, 5);
        Assert.Equal(9, result);

        MathAPI api = new MathAPI();
        invoker = new ApiInvoker(api);
        result = await invoker.InvokeMethodAsync("add", result, 9);
        Assert.Equal(18, result);
    }

    void ValidateCall(FunctionCall call)
    {
        Assert.NotNull(call.Name);
        Assert.NotEmpty(call.Name);
    }

    public static IEnumerable<object[]> GetMathPrograms()
    {
        JsonDocument doc = LoadMathPrograms();
        return GetPrograms(doc);
    }

    public static IEnumerable<object[]> GetStringPrograms()
    {
        JsonDocument doc = LoadStringPrograms();
        return GetPrograms(doc);
    }

    static IEnumerable<object[]> GetPrograms(params JsonDocument[] docs)
    {
        foreach (var doc in docs)
        {
            foreach (var obj in doc.RootElement.EnumerateObject())
            {
                var valueProp = obj.Value.GetProperty("result");
                object result = valueProp.ValueKind == JsonValueKind.Number ?
                                valueProp.GetDouble() :
                                valueProp.GetString();

                var program = obj.Value.GetProperty("source");
                yield return new object[] { program.ToString(), result };
            }
        }
    }
}
