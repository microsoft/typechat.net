// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

public class VocabType : IComparable<VocabType>
{
    string _name;
    IVocab _vocab;

    public VocabType(IVocab vocab)
        : this(string.Empty, vocab)
    {
    }

    public VocabType(string name, IVocab vocab)
    {
        ArgumentNullException.ThrowIfNull(name, nameof(name));
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

    public bool IsAnonymous => string.IsNullOrEmpty(_name);

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
