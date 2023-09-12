// Copyright (c) Microsoft. All rights reserved.
using System.Reflection;
using System.Text.Json;
using System.Linq.Expressions;
using LinqExpression = System.Linq.Expressions.Expression;
using Microsoft.TypeChat.Schema;

namespace Microsoft.TypeChat.Tests;

public class TestProgramCompiler : ProgramTest
{
    [Theory]
    [MemberData(nameof(GetMathPrograms))]
    public void Test_Math(string source, double expectedResult)
    {
        Program program = Json.Parse<Program>(source);
        ProgramCompiler compiler = new ProgramCompiler(typeof(IMathAPI));
        LambdaExpression lambda = compiler.CompileToExpressionTree(program, MathAPI.Default);
        Assert.NotNull(lambda.Body);

        BlockExpression block = lambda.Body as BlockExpression;
        Assert.True(block.Expressions.Count > 0);

        Delegate compiledProgram = lambda.Compile();
        var result = (double)compiledProgram.DynamicInvoke();
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [MemberData(nameof(GetMathProgramsFail))]
    public void TestMath_CompileFail(string source, double expectedResults)
    {
        Program program = Json.Parse<Program>(source);
        ProgramCompiler compiler = new ProgramCompiler(typeof(IMathAPI));
        Assert.ThrowsAny<Exception>(() => compiler.CompileToExpressionTree(program, MathAPI.Default));
    }

    [Theory]
    [MemberData(nameof(GetStringPrograms))]
    public void Test_String(string source, string expectedResult)
    {
        Program program = Json.Parse<Program>(source);

        ProgramCompiler compiler = new ProgramCompiler(typeof(IStringAPI));
        LambdaExpression lambda = compiler.CompileToExpressionTree(program, TextApis.Default);
        Assert.NotNull(lambda.Body);

        Delegate compiledProgram = lambda.Compile();
        var result = compiledProgram.DynamicInvoke();
        Assert.Equal(result, expectedResult);
    }

    /// <summary>
    /// Test object programs
    /// </summary>
    /// <param name="source"></param>
    /// <param name="expectedResults"></param>
    [Theory]
    [MemberData(nameof(GetObjectPrograms))]
    public void Test_Object(string source, string expectedResults)
    {
        Program program = Json.Parse<Program>(source);
        ProgramCompiler compiler = new ProgramCompiler(typeof(IPersonApi));
        LambdaExpression lambda = compiler.CompileToExpressionTree(program, PersonAPI.Default);
        Assert.NotNull(lambda.Body);

        Delegate compiledProgram = lambda.Compile();
        var result = compiledProgram.DynamicInvoke();
        Assert.NotNull(result);
        ValidateResult(result, expectedResults);
    }

    [Fact]
    public void TestCall_JsonObject()
    {
        Person person = new Person
        {
            Name = new Name
            {
                FirstName = "Mario",
                LastName = "Minderbinder"
            },
            Location = new Location
            {
                City = "Barsoom",
                State = "Helium",
                Country = "Mars"
            },
            Age = 24
        };
        dynamic personJson = JsonObject.Parse(Json.Stringify(person));
        string json2 = PersonAPI.Caller.Call("toJson", personJson);
        Person person2 = JsonSerializer.Deserialize<Person>(json2) as Person;
        // This should throw a type mismatch because params are in wrong order
        Assert.ThrowsAny<Exception>(() => PersonAPI.Caller.Call("isPerson", person, person2.Age, person2.Name));
        dynamic result = PersonAPI.Caller.Call("isPerson", person, person2.Name, person2.Age);
        Assert.True(result);
        person2.Name.LastName = "Yossarian";
        result = PersonAPI.Caller.Call("isPerson", person, person2.Name, person2.Age);
        Assert.False(result);
    }
}
