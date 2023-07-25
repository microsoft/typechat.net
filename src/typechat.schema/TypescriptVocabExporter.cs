// Copyright (c) Microsoft. All rights reserved.

using System.Reflection;

namespace Microsoft.TypeChat.Schema;

public class TypescriptVocabExporter
{
    TypescriptWriter _writer;
    HashSet<string> _exported;
    IVocabStore _store;

    public TypescriptVocabExporter(TypescriptWriter writer, IVocabStore store)
    {
        ArgumentNullException.ThrowIfNull(writer, nameof(writer));
        ArgumentNullException.ThrowIfNull(store, nameof(store));

        _writer = writer;
        _store = store;
        _exported = new HashSet<string>();
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

    public bool Export(PropertyInfo property) => ExportMember(property);
    public bool Export(FieldInfo field) => ExportMember(field);

    public void Export(IVocab vocab, string? typeName = null)
    {
        typeName ??= vocab.Name;
        ArgumentException.ThrowIfNullOrEmpty(typeName, nameof(typeName));

        if (IsExported(typeName))
        {
            return;
        }

        _writer.SOL();
        _writer.Type(typeName).Space().Assign().Space();
        {
            ExportValues(vocab);
        }
        _writer.EOS();
        AddExported(vocab);
    }

    bool ExportMember(MemberInfo member)
    {
        ArgumentNullException.ThrowIfNull(member, nameof(member));

        VocabAttribute? vocabAttr = member.GetCustomAttribute(typeof(VocabAttribute)) as VocabAttribute;
        if (vocabAttr == null ||
            !vocabAttr.HasName)
        {
            return false;
        }
        if (IsExported(vocabAttr.Name))
        {
            return true;
        }

        IVocab? vocab = _store.Get(vocabAttr.Name);
        if (vocab == null)
        {
            return false;
        }

        Export(vocab);
        return true;
    }

    void ExportValues(IVocab vocab)
    {
        var values = from entry in vocab
                     select entry.Text;
        _writer.Literals(values);
    }

    bool IsExported(IVocab vocab) => IsExported(vocab.Name);
    bool IsExported(string vocabName) => _exported.Contains(vocabName);

    void AddExported(IVocab vocab)
    {
        _exported.Add(vocab.Name);
    }
}
