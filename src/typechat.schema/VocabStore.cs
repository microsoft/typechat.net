// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

public interface IVocabStore
{
    IVocab? Get(string name);
}

public class VocabStore : Dictionary<string, IVocab>, IVocabStore
{
    public VocabStore()
    {
    }

    public void Add(IVocab vocab)
    {
        ArgumentNullException.ThrowIfNull(vocab, nameof(vocab));
        base.Add(vocab.Name, vocab);
    }

    public IVocab? Get(string name)
    {
        return this.GetValueOrDefault(name, null);
    }
}
