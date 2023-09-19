// Copyright (c) Microsoft. All rights reserved.

using Microsoft.TypeChat.CSharp;

namespace Microsoft.TypeChat.Tests;

public class TestProgramCSharp : ProgramTest
{
    [Theory]
    [MemberData(nameof(GetMathPrograms))]
    public void TestMath(string source, double expectedResult)
    {
        Program program = Json.Parse<Program>(source);
        Api<IMathAPI> api = new MathAPI();
        string code = CSharpProgramTranspiler.GenerateCode(program, api.Type);
        var lines = code.Lines();
        ValidateCode(lines);

        Result<ProgramAssembly> result = CSharpProgramCompiler.Compile(program, api.Type);
        Assert.True(result.Success);

        double mathResult = result.Value.Run(api.Implementation);
        Assert.Equal(expectedResult, mathResult);
    }

    [Theory]
    [MemberData(nameof(GetObjectPrograms))]
    public void TestObject(string source, string expectedResult)
    {
        Program program = Json.Parse<Program>(source);
        Api<IPersonApi> api = new PersonAPI();
        string code = CSharpProgramTranspiler.GenerateCode(program, api.Type);
        var lines = code.Lines();
        ValidateCode(lines);

        Result<ProgramAssembly> result = CSharpProgramCompiler.Compile(program, api.Type);
        Assert.True(result.Success);

        dynamic objResult = result.Value.Run(api.Implementation);
        ValidateResult(objResult, expectedResult);
    }

    [Theory]
    [MemberData(nameof(GetStringPrograms))]
    public void TestString(string source, string expectedResult)
    {
        Program program = Json.Parse<Program>(source);
        Api<IStringAPI> api = new TextApis();
        string code = CSharpProgramTranspiler.GenerateCode(program, api.Type);
        var lines = code.Lines();
        ValidateCode(lines);

        Result<ProgramAssembly> result = CSharpProgramCompiler.Compile(program, api.Type);
        Assert.True(result.Success);

        dynamic objResult = result.Value.Run(api.Implementation);
        Assert.Equal(objResult, expectedResult);
    }

    [Theory]
    [MemberData(nameof(GetStringPrograms))]
    public void TestString_FailWrongApi(string source, string expectedResult)
    {
        // Try to compile against the wrong api
        Program program = Json.Parse<Program>(source);
        Api<IMathAPI> api = MathAPI.Default;
        Assert.ThrowsAny<Exception>(() => CSharpProgramTranspiler.GenerateCode(program, api.Type));
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
