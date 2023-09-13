// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Tests.Netstandard20;

public class TestSchema
{
    public void ValidateBasic(Type type, TypeSchema schema)
    {
        Assert.NotNull(schema);
        Assert.Equal(type, schema.Type);
        Assert.False(string.IsNullOrEmpty(schema.Schema));
    }
    
    [Fact]
    public void TestExportBasic()
    {
        TypeSchema schema = TypescriptExporter.GenerateSchema(typeof(SentimentResponse));
        this.ValidateBasic(typeof(SentimentResponse), schema);
        Assert.True(schema.Schema.Text.Contains("sentiment"));

        schema = TypescriptExporter.GenerateSchema(typeof(Order), TestVocabs.All());
        this.ValidateBasic(typeof(Order), schema);

        var lines = schema.Schema.Text.Lines();
        Assert.True(lines.ContainsSubstring("coffee", "CoffeeOrder[]"));
        Assert.True(lines.ContainsSubstring("desserts", "DessertOrder[]"));
        Assert.True(lines.ContainsSubstring("fruits", "FruitOrder[]"));
    }
    
    [Fact]
    public void TestExportNullable()
    {
        var schema = TypescriptExporter.GenerateSchema(typeof(NullableTestObj));
        // Need better verifier
        var lines = schema.Schema.Text.Lines();
        Assert.False(lines.ContainsSubstring("optionalText?", "string"));
        Assert.True(lines.ContainsSubstring("optionalText", "string"));
        Assert.True(lines.ContainsSubstring("OptionalAmt?", "number"));

        VocabCollection vocabs = new VocabCollection { TestVocabs.Milks() };
        schema = TypescriptExporter.GenerateSchema(typeof(WrapperNullableObj), vocabs);
        lines = schema.Schema.Text.Lines();
        Assert.False(lines.ContainsSubstring("optionalText?", "string"));
        Assert.True(lines.ContainsSubstring("optionalText", "string"));
        Assert.True(lines.ContainsSubstring("OptionalAmt?", "number"));
        Assert.False(lines.ContainsSubstring("milk?"));
        Assert.True(lines.ContainsSubstring("milk"));
    }
}
