// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

public class JsonVocabAttribute : JsonConverterAttribute
{
    string _entries;
    IVocab _vocab;

    public JsonVocabAttribute() { }

    public JsonVocabAttribute(string entries, string? propertyName = null)
    {
        Entries = entries;
        PropertyName = propertyName;
    }

    public string? Name { get; set; }
    public string? Entries
    {
        get => _entries;
        set
        {
            _vocab = null;
            _entries = value;
            if (!string.IsNullOrEmpty(value))
            {
                _vocab = Schema.Vocab.Parse(_entries);
            }
        }
    }

    public string? PropertyName { get; set; }
    public bool Inline { get; set; } = true;
    public IVocab? Vocab => _vocab;

    public bool HasEntries => (!string.IsNullOrEmpty(_entries));
    public bool HasName => !string.IsNullOrEmpty(Name);
    public bool HasVocab => (_vocab != null);
    public bool HasPropertyName => !string.IsNullOrEmpty(PropertyName);

    public override JsonConverter? CreateConverter(Type typeToConvert)
    {
        if (typeof(string) != typeToConvert)
        {
            return null;
        }

        if (HasVocab)
        {
            return new JsonVocabConvertor(_vocab, PropertyName);
        }
        if (HasName)
        {
            return new JsonVocabConvertor(Name, PropertyName);
        }
        return base.CreateConverter(typeToConvert);
    }

    public Vocab? ToVocab()
    {
        if (!HasEntries)
        {
            return null;
        }
        return new Vocab(_vocab);
    }

    public VocabType? ToVocabType()
    {
        if (!HasVocab)
        {
            return null;
        }
        return new VocabType(Name, _vocab);
    }
}

