// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Tests;

public class TestVocab : TypeChatTest
{
    [Fact]
    public void TestVocabAttribute()
    {
        VocabAttribute attribute = new VocabAttribute("One | Two | Three", "Test");
        Vocab? vocab = attribute.ToVocab();
        Assert.NotNull(vocab);
        Assert.True(vocab.Count == 3);

        VocabType? type = attribute.ToVocabType();
        Assert.NotNull(type);
        Assert.Equal(type.Name, attribute.Name);
        Assert.Equal(vocab, type.Vocab);
    }

    [Fact]
    public void ExportLocal()
    {
        VocabAttribute? vattr = typeof(LocalVocabObj).GetProperties()[0].VocabAttribute();
        Assert.NotNull(vattr);
        Vocab vocab = vattr.ToVocab();

        using StringWriter writer = new StringWriter();
        TypescriptExporter exporter = new TypescriptExporter(writer);
        exporter.Export(typeof(LocalVocabObj));
        Assert.NotNull(exporter.UsedVocabs);
        Assert.True(exporter.UsedVocabs.Count == 1);
        IVocab localVocab = exporter.UsedVocabs.Get(LocalVocabObj.VocabName).Vocab;
        Assert.Equal(vocab, localVocab);
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
    public void ExportVocabInline()
    {
        var milks = TestVocabs.Milks();
        VocabCollection vocabs = new VocabCollection { milks };
        var schema = TypescriptExporter.GenerateSchema(typeof(Milk), vocabs);
        ValidateBasic(typeof(Milk), schema);
        ValidateVocabInline(schema, milks);
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
    public async Task TestVocabFile()
    {
        VocabCollection vocabs = await VocabFile.LoadAsync("TestVocabs.json");
        Assert.True(vocabs.Contains("Desserts"));
        Assert.True(vocabs.Contains("Fruits"));

        var fruitType = vocabs.Get("Fruits");
        Assert.NotNull(fruitType);
        Assert.Contains("Banana", fruitType.Vocab);
        Assert.Contains("Pear", fruitType.Vocab);
    }
}
