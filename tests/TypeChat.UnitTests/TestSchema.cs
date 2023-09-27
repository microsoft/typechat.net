// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Tests;

public class TestSchema : TypeChatTest
{
    [Fact]
    public void ExportBasic()
    {
        TypeSchema schema = TypescriptExporter.GenerateSchema(typeof(SentimentResponse));
        ValidateBasic(typeof(SentimentResponse), schema);
        Assert.True(schema.Schema.Text.Contains("sentiment"));

        schema = TypescriptExporter.GenerateSchema(typeof(Order), TestVocabs.All());
        ValidateBasic(typeof(Order), schema);

        var lines = schema.Schema.Text.Lines();
        Assert.True(lines.ContainsSubstring("coffee", "CoffeeOrder[]"));
        Assert.True(lines.ContainsSubstring("desserts", "DessertOrder[]"));
        Assert.True(lines.ContainsSubstring("fruits", "FruitOrder[]"));
    }

    [Fact]
    public void ExportNullable()
    {
        var schema = TypescriptExporter.GenerateSchema(typeof(NullableTestObj));
        // Need better verifier
        var lines = schema.Schema.Text.Lines();
        Assert.True(lines.ContainsSubstring("optionalText?", "string"));
        Assert.True(lines.ContainsSubstring("OptionalAmt?", "number"));

        VocabCollection vocabs = new VocabCollection { TestVocabs.Milks() };
        schema = TypescriptExporter.GenerateSchema(typeof(WrapperNullableObj), vocabs);
        lines = schema.Schema.Text.Lines();
        Assert.True(lines.ContainsSubstring("optionalText?", "string"));
        Assert.True(lines.ContainsSubstring("OptionalAmt?", "number"));
        Assert.True(lines.ContainsSubstring("milk?"));
    }

    [Fact]
    public void ExportMathAPI()
    {
        var schema = TypescriptExporter.GenerateAPI(typeof(IMathAPI));
        // Need better verifier
        var lines = schema.Schema.Text.Lines();
        Assert.True(lines.ContainsSubstring("interface", "IMathAPI"));
        Assert.True(lines.ContainsSubstring("number", "add", "x", "y"));
    }

    [Fact]
    public void ExportStringAPI()
    {
        var schema = TypescriptExporter.GenerateAPI(typeof(IStringAPI));
        // Need better verifier
        var lines = schema.Schema.Text.Lines();
        Assert.True(lines.ContainsSubstring("interface", "IStringAPI"));
        Assert.True(lines.ContainsSubstring("string", "concat", "args", "any"));
        Assert.True(lines.ContainsSubstring("string", "lowercase", "string"));
    }

    [Fact]
    public void ExportObjectAPI()
    {
        var schema = TypescriptExporter.GenerateAPI(typeof(IPersonApi));
        // Need better verifier
        var lines = schema.Schema.Text.Lines();
        Assert.True(lines.ContainsSubstring("interface", "IPersonApi"));
        Assert.True(lines.ContainsSubstring("toJsonArray", "Person[]"));
        Assert.True(lines.ContainsSubstring("interface", "Person"));
        Assert.True(lines.ContainsSubstring("interface", "Name"));
        Assert.True(lines.ContainsSubstring("interface", "Location"));
    }
}

