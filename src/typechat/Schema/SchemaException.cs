// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

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
}


