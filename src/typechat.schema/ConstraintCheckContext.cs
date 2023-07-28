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

    public bool CheckVocabEntry(string propertyName, string vocabName, string value)
    {
        VocabType? vocabType = _vocabs.Get(vocabName);
        if (vocabType == null)
        {
            return false;
        }
        string? validationResult = vocabType.Vocab.ValidateConstraints(propertyName, value);
        if (string.IsNullOrEmpty(validationResult))
        {
            return true;
        }
        _errorWriter.WriteLine(validationResult);
        return false;
    }
}
