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

        schema = TypescriptExporter.GenerateSchema(typeof(Order));
        ValidateBasic(typeof(Order), schema);
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

    void ValidateVocab(string text, IVocab vocab)
    {
        // Kludgy for now
        foreach (var entry in vocab)
        {
            Assert.True(text.Contains($"'{entry}'"));
        }
    }
}

