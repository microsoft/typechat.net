﻿// Copyright (c) Microsoft. All rights reserved.

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

        StringAPI api = new StringAPI();
        ProgramInterpreter interpreter = new ProgramInterpreter(api.HandleCall);
        string result = interpreter.Run(program);
        Assert.Equal(expectedResult, result);
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
