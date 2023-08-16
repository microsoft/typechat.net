// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

public static class ConstraintValidationEx
{
    internal static void ValidateConstraints(this object obj, ConstraintCheckContext context)
    {
        if (obj != null &&
            obj is IConstraintValidatable validator)
        {
            validator.ValidateConstraints(context);
        }
    }

    internal static void ValidateConstraints(this IEnumerable<object> objects, ConstraintCheckContext context)
    {
        if (objects != null)
        {
            foreach (var obj in objects)
            {
                if (obj is IConstraintValidatable validator)
                {
                    validator.ValidateConstraints(context);
                }
            }
        }
    }

    public static void ThrowIfNotInVocab(this IVocab vocab, string? propertyName, string? value)
    {
        if (vocab == null)
        {
            throw new SchemaException(SchemaException.ErrorCode.VocabNotFound, $"{value} is not a known value");
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

        return $"REMAP {value} to one of: {vocab}";
    }

    public static string? ValidateConstraints(this IVocab vocab, string propertyName, string value)
    {
        if (vocab.Contains(value))
        {
            return null;
        }

        return $"{propertyName}: REMAP '{value}' to one of: {vocab}";
    }

    public static void ThrowIfNotInVocab(this IVocabCollection vocabs, string vocabName, string? propertyName, string? value)
    {
        VocabType? vocabType = vocabs.Get(vocabName);
        if (vocabType == null)
        {
            throw new SchemaException(SchemaException.ErrorCode.VocabNotFound, vocabName);
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
