// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Tests;

public class TestProgramCSharp : ProgramTest
{
    [Theory]
    [MemberData(nameof(GetMathPrograms))]
    public void Test_Math(string source, double expectedResult)
    {
        Program program = Json.Parse<Program>(source);
        string code = CSharpProgramWriter.GenerateCode(program);
        var lines = code.Lines();
        ValidateCode(lines);

        Compile(code);
    }

    void Compile(string code)
    {
        CSharpProgramCompiler compiler = new CSharpProgramCompiler();
        var result = compiler.Compile(code);
        Assert.True(result.Success);
    }

    void ValidateCode(IEnumerable<string> lines)
    {
        ValidateStandardUsings(lines);
    }

    void ValidateStandardUsings(IEnumerable<string> lines)
    {
        Assert.True(lines.ContainsSubstring("using ", "System"));
    }
}
