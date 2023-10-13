// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

public class TypescriptSchema : TypeSchema
{
    public TypescriptSchema(Type type, string schemaText, IVocabCollection? vocabs = null)
        : base(type, new SchemaText(schemaText, SchemaText.Languages.Typescript))
    {
        Vocabs = vocabs;
    }

    public IVocabCollection? Vocabs { get; }

    public bool HasVocabs => Vocabs is not null;

    public static TypescriptSchema Load(Type type, string filePath)
        => new TypescriptSchema(type, File.ReadAllText(filePath));
}
