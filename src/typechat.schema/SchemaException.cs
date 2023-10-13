// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

/// <summary>
/// Exception thrown if Typechat encounters a schema issue
/// </summary>
public class SchemaException : Exception
{
    public enum ErrorCode
    {
        VocabNotFound,
        ValueNotInVocab
    }

    public SchemaException(string? message, Exception? inner = null)
        : base(message, inner)
    {
    }

    public SchemaException(ErrorCode code, string? message = null, Exception? inner = null)
        : base(message, inner)
    {
        Code = code;
    }

    public ErrorCode Code { get; }

    public override string ToString()
    {
        return $"Error: {Code}\n{Message}";
    }

    public static void ThrowVocabNotFound(string vocabName)
    {
        throw new SchemaException(SchemaException.ErrorCode.VocabNotFound, $"Vocabulary {vocabName} is not a known value");
    }

    public static void ThrowVocabNotFound(string vocabName, string value)
    {
        throw new SchemaException(SchemaException.ErrorCode.VocabNotFound, $"{value} is from an unknown vocabulary {vocabName}");
    }
}


