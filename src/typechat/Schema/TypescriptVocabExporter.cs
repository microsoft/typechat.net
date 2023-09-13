// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

public class TypescriptVocabExporter : TypeExporter<NamedVocab>
{
    TypescriptWriter _writer;
    IVocabCollection _store;

    public TypescriptVocabExporter(TypescriptWriter writer, IVocabCollection store)
        : base()
    {
        ArgumentNullException.ThrowIfNull(writer, nameof(writer));
        ArgumentNullException.ThrowIfNull(store, nameof(store));

        _writer = writer;
        _store = store;
    }

    public IVocabCollection Vocabs => _store;

    public override void ExportType(NamedVocab type)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));

        if (IsExported(type))
        {
            return;
        }

        _writer.SOL();
        _writer.Type(type.Name).Space().Assign().Space();
        {
            ExportValues(type.Vocab);
        }
        _writer.EOS();
        AddExported(type);
    }

    void ExportValues(IVocab vocab)
    {
        var values = from entry in vocab
                     select entry.Text;
        _writer.Literals(values);
    }
}
