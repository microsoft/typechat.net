// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

public class TypescriptExportException : Exception
{
    public enum ErrorCode
    {
        VocabNotFound
    }

    ErrorCode _errorCode;

    public TypescriptExportException(string? message, Exception? inner = null)
        : base(message, inner)
    {
    }

    public TypescriptExportException(ErrorCode code, string? message, Exception? inner = null)
        : base(MakeMessage(code, message), inner)
    {
        _errorCode = code;
    }

    public ErrorCode Code => _errorCode;

    static string MakeMessage(ErrorCode code, string? message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return $"Error: {code}";
        }
        return $"Error: {code} \n{message}";
    }
}


