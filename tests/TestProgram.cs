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
        Assert.NotNull(program.Steps);
        Assert.NotNull(program.Steps.Calls);
        Assert.True(program.Steps.Calls.Length > 0);
    }

    public static IEnumerable<object[]> GetTestPrograms()
    {
        JsonDocument doc = LoadPrograms();
        foreach (var obj in doc.RootElement.EnumerateObject())
        {
            var program = obj.Value.ToString();
            yield return new[] { program };
        }
    }
}
