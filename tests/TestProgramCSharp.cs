// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Tests;

public class TestProgramCSharp : ProgramTest
{
    [Theory]
    [MemberData(nameof(GetMathPrograms))]
    public void Test_Math(string source, double expectedResult)
    {
        Program program = Json.Parse<Program>(source);
        string code = CSharpProgramWriter.GenerateCode(program, typeof(IMathAPI));
        var lines = code.Lines();
        ValidateCode(lines);

        Result<ProgramAssembly> result = CSharpProgramCompiler.Compile(program, typeof(IMathAPI));
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
