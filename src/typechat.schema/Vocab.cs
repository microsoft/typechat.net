// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

public interface IVocab : IEnumerable<VocabEntry>
{
    string Name { get; }

    bool Contains(VocabEntry entry);
}

public class Vocab : SortedSet<VocabEntry>, IVocab
{
    string _name;

    public Vocab(string name)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(name, nameof(name));
        _name = name;
    }

    public string Name => _name;
}
