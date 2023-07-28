// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

public class VocabType : IComparable<VocabType>
{
    string _name;
    IVocab _vocab;

    public VocabType(string name, IVocab vocab)
    {
        ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));
        ArgumentNullException.ThrowIfNull(vocab, nameof(vocab));

        _name = name;
        _vocab = vocab;
    }

    [JsonConstructor]
    public VocabType(string name, Vocab vocab)
        : this(name, (IVocab)vocab)
    {
    }

    public string Name => _name;
    public IVocab Vocab => _vocab;

    public int CompareTo(VocabType? other)
    {
        return _name.CompareTo(other._name);
    }

    public override bool Equals(object? obj)
    {
        return _name.Equals(obj);
    }

    public override int GetHashCode()
    {
        return _name.GetHashCode();
    }

    public override string? ToString()
    {
        return _name;
    }
}
