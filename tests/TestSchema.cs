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
        Assert.True(lines.Contains("coffee", "CoffeeOrder[]"));
        Assert.True(lines.Contains("desserts", "DessertOrder[]"));
        Assert.True(lines.Contains("fruits", "FruitOrder[]"));
    }

    [Fact]
    public void ExportNullable()
    {
        var schema = TypescriptExporter.GenerateSchema(typeof(NullableTestObj));
        // Need better verifier
        var lines = schema.Schema.Text.Lines();
        Assert.True(lines.Contains("optionalText?", "string"));
        Assert.True(lines.Contains("OptionalAmt?", "number"));

        VocabCollection vocabs = new VocabCollection { TestVocabs.Milks() };
        schema = TypescriptExporter.GenerateSchema(typeof(WrapperNullableObj), vocabs);
        lines = schema.Schema.Text.Lines();
        Assert.True(lines.Contains("optionalText?", "string"));
        Assert.True(lines.Contains("OptionalAmt?", "number"));
        Assert.True(lines.Contains("milk?"));
    }

    [Fact]
    public void ExportAPI()
    {
        var schema = TypescriptExporter.GenerateAPI(typeof(IMathAPI));
        // Need better verifier
        var lines = schema.Schema.Text.Lines();
        Assert.True(lines.Contains("interface", "IMathAPI"));
        Assert.True(lines.Contains("number", "add", "x", "y"));
        Assert.True(lines.Contains("number", "unknown", "string", "text"));

        schema = TypescriptExporter.GenerateAPI(typeof(IStringAPI));
        // Need better verifier
        lines = schema.Schema.Text.Lines();
        Assert.True(lines.Contains("interface", "IStringAPI"));
        Assert.True(lines.Contains("string", "concat", "args"));
        Assert.True(lines.Contains("string", "lowercase", "string"));
    }
}

