// Copyright (c) Microsoft. All rights reserved.

using System.Text.Json;

namespace Microsoft.TypeChat.Tests;

public class TestProgram : TypeChatTest
{
    static JsonDocument LoadPrograms()
    {
        string json = File.ReadAllText("testPrograms.json");
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
    [MemberData(nameof(GetTestPrograms))]
    public void TestParse(string source)
    {
        Program program = new ProgramParser().Parse(source);
        ValidateProgram(program);
    }

    [Theory]
    [MemberData(nameof(GetTestPrograms))]
    public void TestJsonConvert(string source)
    {
        Program program = Json.Parse<Program>(source);
        ValidateProgram(program);
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

    void ValidateCall(Call call)
    {
        Assert.NotNull(call.Name);
        Assert.NotEmpty(call.Name);
    }

    public static IEnumerable<object[]> GetTestPrograms()
    {
        JsonDocument doc = LoadPrograms();
        foreach (var obj in doc.RootElement.EnumerateObject())
        {
            var program = obj.Value.GetProperty("source");
            yield return new[] { program.ToString() };
        }
    }
}
