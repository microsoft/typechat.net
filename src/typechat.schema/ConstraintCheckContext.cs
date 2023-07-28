// Copyright (c) Microsoft. All rights reserved.

using System.Text;

namespace Microsoft.TypeChat.Schema;

public class ConstraintCheckContext
{
    TextWriter _errorWriter;
    IVocabCollection _vocabs;

    public ConstraintCheckContext(TextWriter errorWriter, IVocabCollection? vocabs)
    {
        ArgumentNullException.ThrowIfNull(errorWriter, nameof(errorWriter));
        _errorWriter = errorWriter;
        _vocabs = vocabs;
    }

    public TextWriter Error => _errorWriter;
    public IVocabCollection? Vocabs => _vocabs;

    public bool CheckVocabEntry(string propertyName, string vocabName, string entry)
    {
        VocabType? vocabType = _vocabs.Get(vocabName);
        if (vocabType != null &&
            vocabType.Vocab.Contains(entry))
        {
            return true;
        }

        _errorWriter.WriteLine($"{propertyName}: REMAP '{entry}' to one of: {VocabEntries(vocabType.Vocab)}");
        return false;
    }

    string VocabEntries(IVocab vocab)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var value in vocab.Strings())
        {
            if (sb.Length > 0)
            {
                sb.Append(" | ");
            }
            sb.Append('\'').Append(value).Append('\'');
        }
        return sb.ToString();
    }
}
