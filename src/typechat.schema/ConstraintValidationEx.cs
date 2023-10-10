// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

internal static class ConstraintValidationEx
{
    public static void ThrowIfNotInVocab(this IVocab vocab, string? propertyName, string? value)
    {
        if (vocab == null)
        {
            throw new SchemaException(SchemaException.ErrorCode.VocabNotFound);
        }
        string? error;
        if (propertyName == null)
        {
            error = vocab.ValidateConstraints(value);
        }
        else
        {
            error = vocab.ValidateConstraints(propertyName, value);
        }
        if (error != null)
        {
            throw new SchemaException(SchemaException.ErrorCode.ValueNotInVocab, error);
        }
    }

    public static string? ValidateConstraints(this IVocab vocab, string value)
    {
        if (vocab.Contains(value))
        {
            return null;
        }

        return $"'{value}' does not exist. Permitted values: {vocab}";
    }

    public static string? ValidateConstraints(this IVocab vocab, string propertyName, string value)
    {
        if (vocab.Contains(value))
        {
            return null;
        }

        return $"{propertyName} '{value}' does not exist. Permitted {propertyName} values: {vocab}";
    }

    public static void ThrowIfNotInVocab(this IVocabCollection vocabs, string vocabName, string? propertyName, string? value)
    {
        NamedVocab? vocabType = vocabs.Get(vocabName);
        if (vocabType == null)
        {
            SchemaException.ThrowVocabNotFound(vocabName, value);
        }
        string? error;
        if (propertyName == null)
        {
            error = vocabType.Vocab.ValidateConstraints(value);
        }
        else
        {
            error = vocabType.Vocab.ValidateConstraints(propertyName, value);
        }
        if (error != null)
        {
            throw new SchemaException(SchemaException.ErrorCode.ValueNotInVocab, error);
        }
    }
}
