// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Tests;

public class TypeChatTest
{
    public string? GetEnv(string name)
    {
        return Environment.GetEnvironmentVariable(name);
    }

    public string? SetEnv(string name, string value)
    {
        string? prev = GetEnv(name);
        Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.Process);
        return prev;
    }

    public void ClearEnv(string name)
    {
        Environment.SetEnvironmentVariable(name, null, EnvironmentVariableTarget.Process);
    }

    // *Very* basic checks.
    // Need actual robust validation, e.g. by loading in Typescript
    //   
    public void ValidateBasic(Type type, TypeSchema schema)
    {
        Assert.NotNull(schema);
        Assert.Equal(type, schema.Type);
        Assert.False(string.IsNullOrEmpty(schema.Schema));
    }

    public void ValidateVocab(TypeSchema schema, VocabType vocab)
    {
        ValidateVocab(schema.Schema.Text, vocab.Vocab);
    }

    public void ValidateVocabInline(TypeSchema schema, VocabType vocab)
    {
        // Type should not be emitted. Kludgy test
        Assert.False(schema.Schema.Text.Contains(vocab.Name));
        ValidateVocab(schema.Schema.Text, vocab.Vocab);
    }

    public void ValidateVocab(string text, IVocab vocab)
    {
        // Kludgy for now
        foreach (var entry in vocab)
        {
            Assert.True(text.Contains($"'{entry}'"));
        }
    }

}
