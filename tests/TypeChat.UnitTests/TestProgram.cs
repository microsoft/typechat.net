// Copyright (c) Microsoft. All rights reserved.

using System.Reflection;

namespace Microsoft.TypeChat.Tests;

public class TestProgram : ProgramTest
{
    //[Fact]
    public void TestSchema()
    {
        TypescriptSchema schema = TypescriptExporter.GenerateSchema(typeof(Program));
        var lines = schema.Schema.Text.ReadLines();
        Assert.True(lines.ContainsSubstring("steps", "Call"));
        Assert.True(lines.ContainsSubstring("func", "string"));
        Assert.True(lines.ContainsSubstring("args", "Expr[]"));
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

    //[Fact]
    public void TestDynamic()
    {
        dynamic[] args = new dynamic[2];
        args[0] = 3;
        args[1] = 4;
        dynamic result = args[0] + args[1];
        Assert.Equal(7, result);

        MathAPI api = new MathAPI();
        MethodInfo addMethod = GetMethod(api.GetType(), "add");
        result = addMethod.Invoke(api, args);
        Assert.Equal(7, result);
        JsonNode node = result;
        Assert.Equal(7, (double)node);

        args[0] = "Mario";
        Assert.Equal("Mario4", args[0] + args[1]);
        args[1] = "_Minderbinder";
        Assert.Equal("Mario_Minderbinder", args[0] + args[1]);
    }

    [Fact]
    public void TestToJson()
    {
        dynamic[] items = new dynamic[2];
        items[0] = "Hello";
        items[1] = "Goodbye";

        JsonNode node = ProgramInterpreter.ToJsonNode(items);
        Assert.True(node is JsonArray);

        node = ProgramInterpreter.ToJsonNode("Hello");
        Assert.False(node is JsonArray);

        int[] numbers = new int[2];
        numbers[0] = 1;
        numbers[1] = 2;
        node = ProgramInterpreter.ToJsonNode(numbers);
        Assert.True(node is JsonArray);

        JsonObject jsonObj = new JsonObject();
        jsonObj.Add("numbers", node);
        jsonObj.Add("Title", "Yo");

        node = ProgramInterpreter.ToJsonNode(jsonObj);
        Assert.True(node is JsonObject);
    }

    [Theory]
    [MemberData(nameof(GetStringPrograms))]
    public void TestProgramValidator_String(string source, string expectedResult)
    {
        Program program = Json.Parse<Program>(source);
        var validator = new ProgramValidator<IStringAPI>(TextApis.Default);
        validator.ValidateProgram(program);
    }

    [Theory]
    [MemberData(nameof(GetMathPrograms))]
    public void TestProgramValidator_Math(string source, string expectedResult)
    {
        Program program = Json.Parse<Program>(source);
        var validator = new ProgramValidator<IMathAPI>(MathAPI.Default);
        validator.ValidateProgram(program);
    }

    [Fact]
    public async Task TestAsync()
    {
        MathAPIAsync mathAsync = MathAPIAsync.Default;
        Api invoker = new Api(mathAsync);
        double result = await invoker.CallAsync("add", 4, 5);
        Assert.Equal(9, result);

        MathAPI api = new MathAPI();
        invoker = new Api(api);
        result = await invoker.CallAsync("add", result, 9);
        Assert.Equal(18, result);
    }

    [Fact]
    public void TestAsyncApi()
    {
        Api<IAsyncService> api = new EmojiApi();
        ILanguageModel model = new MockLanguageModel();
        // This should not throw
        ProgramTranslator<IAsyncService> service = new ProgramTranslator<IAsyncService>(model, api);
    }
}
