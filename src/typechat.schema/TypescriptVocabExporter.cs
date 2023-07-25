// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

public class TypescriptVocabExporter : TypeExporter<VocabType>
{
    TypescriptWriter _writer;
    IVocabStore _store;

    public TypescriptVocabExporter(TypescriptWriter writer, IVocabStore store)
        : base()
    {
        ArgumentNullException.ThrowIfNull(writer, nameof(writer));
        ArgumentNullException.ThrowIfNull(store, nameof(store));

        _writer = writer;
        _store = store;
    }

    public IVocabStore Vocabs
    {
        get => _store;
        set
        {
            ArgumentNullException.ThrowIfNull(value, nameof(Vocabs));
            _store = value;
        }
    }

    public bool AddPending(PropertyInfo property) => AddPending(property);
    public bool AddPending(FieldInfo field) => AddPending(field);

    public override void Export(VocabType type)
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

    bool AddPending(MemberInfo member)
    {
        VocabType? vocab = _store.VocabFor(member);
        if (vocab == null ||
            IsExported(vocab))
        {
            return false;
        }

        AddPending(vocab);
        return true;
    }

    void ExportValues(IVocab vocab)
    {
        var values = from entry in vocab
                     select entry.Text;
        _writer.Literals(values);
    }
}
