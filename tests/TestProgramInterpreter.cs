﻿// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Tests;

public class TestProgramInterpreter : ProgramTest
{
    // This test is basically the Math example turned into a unit test
    // Json Programs wer pregenerated by the AI or written by hand
    [Theory]
    [MemberData(nameof(GetMathPrograms))]
    public void Test_Math(string source, double expectedResult)
    {
        Program program = Json.Parse<Program>(source);
        ValidateProgram(program);

        Api api = new Api(new MathAPI());
        double result = program.Run(api);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [MemberData(nameof(GetMathPrograms))]
    public async Task Test_MathAsync(string source, double expectedResult)
    {
        Program program = Json.Parse<Program>(source);
        ValidateProgram(program);

        Api api = new Api(MathAPIAsync.Default);
        ProgramCompiler compiler = new ProgramCompiler(api.TypeInfo);
        Delegate d = compiler.Compile(program, api);
        double result = (double)d.DynamicInvoke();
        Assert.Equal(expectedResult, result);

        result = (double)await program.RunAsync(api);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [MemberData(nameof(GetStringPrograms))]
    public void Test_String(string source, string expectedResult)
    {
        Program program = Json.Parse<Program>(source);
        ValidateProgram(program);

        Api api = new Api(new TextApis());
        string result = program.Run(api);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [MemberData(nameof(GetObjectPrograms))]
    public void Test_Object(string source, string expectedResult)
    {
        Program program = Json.Parse<Program>(source);
        ValidateProgram(program);

        Api api = new Api(new PersonAPI());
        dynamic result = program.Run(api);
        string resultstring;
        if (result is string)
        {
            resultstring = (string)result;
        }
        else
        {
            resultstring = Json.Stringify(result);
        }
        if (!string.IsNullOrEmpty(expectedResult))
        {
            Assert.Equal(expectedResult, resultstring);
        }
    }
}
