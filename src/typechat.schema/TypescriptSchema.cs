// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

public class TypescriptSchema : TypeSchema
{
    IVocabCollection? _vocabs;

    public TypescriptSchema(Type type, string schemaText, IVocabCollection? vocabs)
        : base(type, new SchemaText(schemaText, SchemaText.Languages.Typescript))
    {
        _vocabs = vocabs;
    }

    public IVocabCollection? Vocabs => _vocabs;
}
