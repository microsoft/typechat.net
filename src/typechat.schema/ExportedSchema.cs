// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

public class ExportedSchema : TypeSchema
{
    IVocabCollection? _vocabs;

    public ExportedSchema(Type type, string schemaText, IVocabCollection? vocabs)
        : base(type, schemaText)
    {
        _vocabs = vocabs;
    }

    public IVocabCollection? Vocabs => _vocabs;
}
