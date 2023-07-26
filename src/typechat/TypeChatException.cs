// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public class TypeChatException : Exception
{
    public enum ErrorCode
    {
        Unknown,
        InvalidJson,
        JsonValidation
    }

    ErrorCode _errorCode;

    public TypeChatException(string? message, Exception? inner = null)
        : base(message, inner)
    {
    }

    public TypeChatException(ErrorCode code, string? message, Exception? inner = null)
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
