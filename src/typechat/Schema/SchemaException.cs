﻿// Copyright (c) Microsoft. All rights reserved.

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

    ErrorCode _errorCode;

    public SchemaException(string? message, Exception? inner = null)
        : base(message, inner)
    {
    }

    public SchemaException(ErrorCode code, string? message = null, Exception? inner = null)
        : base(message, inner)
    {
        _errorCode = code;
    }

    public ErrorCode Code => _errorCode;

    public override string ToString()
    {
        return $"Error: {Code}\n{Message}";
    }

    internal static void ThrowVocabNotFound(string vocabName)
    {
        throw new SchemaException(SchemaException.ErrorCode.VocabNotFound, $"Vocabulary {vocabName} is not a known value");
    }

    internal static void ThrowVocabNotFound(string vocabName, string value)
    {
        throw new SchemaException(SchemaException.ErrorCode.VocabNotFound, $"{value} is from an unknown vocabulary {vocabName}");
    }
}


