// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Tests;

public class SchemaTests : TypeChatTest
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
    public void ExportVocab()
    {
        var dessertVocab = TestVocabs.Desserts();
        var fruitsVocab = TestVocabs.Fruits();

        VocabCollection vocabs = new VocabCollection { dessertVocab, fruitsVocab };
        var schema = TypescriptExporter.GenerateSchema(typeof(DessertOrder), vocabs);
        ValidateBasic(typeof(DessertOrder), schema);
        ValidateVocab(schema, dessertVocab);

        schema = TypescriptExporter.GenerateSchema(typeof(Order), vocabs);
        ValidateBasic(typeof(Order), schema);
        ValidateVocab(schema, dessertVocab);
        ValidateVocab(schema, fruitsVocab);
    }

    [Fact]
    public void ExportVocabInline()
    {
        var milks = TestVocabs.Milks();
        VocabCollection vocabs = new VocabCollection { milks };
        var schema = TypescriptExporter.GenerateSchema(typeof(Milk), vocabs);
        ValidateBasic(typeof(Milk), schema);
        ValidateVocabInline(schema, milks);
    }

    [Fact]
    public void ExportVocabDirect()
    {
        string vocabName = "Foo";
        using StringWriter sw = new StringWriter();
        VocabCollection store = new VocabCollection
        {
            {vocabName, new Vocab("One", "Two", "Three") }
        };
        VocabType? type = store.Get(vocabName);
        Assert.NotNull(type);

        var exporter = new TypescriptVocabExporter(new TypescriptWriter(sw), store);
        exporter.Export(type);
        string text = sw.ToString();
        Assert.NotEmpty(text);

        // TODO: better checks for correctness
        Assert.EndsWith(";", text.Trim());
        ValidateVocab(text, type.Vocab);
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

    // *Very* basic checks.
    // Need actual robust validation, e.g. by loading in Typescript
    //   
    void ValidateBasic(Type type, TypeSchema schema)
    {
        Assert.NotNull(schema);
        Assert.Equal(type, schema.Type);
        Assert.False(string.IsNullOrEmpty(schema.Schema));
    }

    void ValidateVocab(TypeSchema schema, VocabType vocab)
    {
        ValidateVocab(schema.Schema.Text, vocab.Vocab);
    }

    void ValidateVocabInline(TypeSchema schema, VocabType vocab)
    {
        // Type should not be emitted. Kludgy test
        Assert.False(schema.Schema.Text.Contains(vocab.Name));
        ValidateVocab(schema.Schema.Text, vocab.Vocab);
    }

    void ValidateVocab(string text, IVocab vocab)
    {
        // Kludgy for now
        foreach (var entry in vocab)
        {
            Assert.True(text.Contains($"'{entry}'"));
        }
    }
}

