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
        string vocabName = "Foo";
        using StringWriter sw = new StringWriter();
        VocabStore store = new VocabStore
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
        foreach(var entry in type.Vocab)
        {
            Assert.True(text.Contains($"'{entry}'"));
        }
    }
}

