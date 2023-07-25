// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Tests;

public class SchemaTests : TypeChatTest
{
    [Fact]
    public void ExportText()
    {
        TypeSchema schema = TypescriptExporter.GenerateSchema(typeof(Order));
        Assert.NotNull(schema);
        Assert.Equal(typeof(Order), schema.Type);
        Assert.False(string.IsNullOrEmpty(schema.Schema));
    }

    [Fact]
    public void ExportVocab()
    {
        Vocab vocab = new Vocab("Foo")
        {
            "One",
            "Two",
            "Three"
        };

        using StringWriter sw = new StringWriter();
        VocabStore store = new VocabStore();
        store.Add(vocab);
        var exporter = new TypescriptVocabExporter(new TypescriptWriter(sw), store);
        exporter.Export(vocab);
        string text = sw.ToString();
        // This test is just checking to ensure no errors...
        // TODO: better checks for correctness
        Assert.EndsWith(";", text.Trim());
        foreach(var entry in vocab)
        {
            Assert.True(text.Contains($"'{entry}'"));
        }
    }
}

