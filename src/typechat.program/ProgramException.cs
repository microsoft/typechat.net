// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public class ProgramException : Exception
{
    public enum ErrorCode
    {
        InvalidProgram,
        UnsupportedType,
        TypeMistmatch,
        InvalidResultRef,
        NoResult,
        InvalidArgCount,
    }

    ErrorCode _errorCode;

    public ProgramException(ErrorCode code, string? message = null, Exception? inner = null)
        : base(message, inner)
    {
        _errorCode = code;
    }

    public ErrorCode Code => _errorCode;
}
