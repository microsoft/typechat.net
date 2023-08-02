// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Tests;

public class TestProgram : TypeChatTest
{
    [Fact]
    public void TestSchema()
    {
        TypescriptSchema schema = TypescriptExporter.GenerateSchema(typeof(Program));
        var lines = schema.Schema.Text.Lines();
        Assert.True(lines.Contains("steps", "Call"));
        Assert.True(lines.Contains("func", "string"));
        Assert.True(lines.Contains("args", "Expr[]"));
    }

    [Fact]
    public void TestExpr()
    {

    }
}
