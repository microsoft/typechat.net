// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

/// <summary>
/// A backing field for strings that must be from a particular vocabulary
/// </summary>
public struct VocabField
{
    string _vocabName;
    string _value;

    public VocabField(string vocabName)
    {
        _vocabName = vocabName;
    }

    public string VocabName => _vocabName;

    public string Value
    {
        get => _value;
        set => _value = value;
    }

    public static implicit operator string(VocabField field)
    {
        return field._value;
    }
}
