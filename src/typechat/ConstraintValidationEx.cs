// Copyright (c) Microsoft. All rights reserved.

using Microsoft.TypeChat.Schema;

namespace Microsoft.TypeChat;

internal static class ConstraintValidationEx
{
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
}
