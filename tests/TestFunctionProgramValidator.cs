// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Tests;

public class TestFunctionProgramValidator : ProgramTest
{
    [Theory]
    [MemberData(nameof(GetMathPrograms))]
    public void Test_Math(string source, double expectedResult)
    {
        Program program = Json.Parse<Program>(source);
        FunctionCallValidator<IMathAPI> validator = new FunctionCallValidator<IMathAPI>(MathAPI.Default);
        validator.ValidateProgram(program);
    }

    [Theory]
    [MemberData(nameof(GetMathProgramsFail))]
    public void TestMath_CompileFail(string source, double expectedResults)
    {
        Program program = Json.Parse<Program>(source);
        FunctionCallValidator<IMathAPI> validator = new FunctionCallValidator<IMathAPI>(MathAPI.Default);
        Assert.False(validator.ValidateProgram(program).Success);
    }

    [Theory]
    [MemberData(nameof(GetObjectPrograms))]
    public void Test_Object(string source, string expectedResults)
    {
        Program program = Json.Parse<Program>(source);
        FunctionCallValidator<IPersonApi> validator = new FunctionCallValidator<IPersonApi>(PersonAPI.Default);
        validator.ValidateProgram(program);
    }
}
