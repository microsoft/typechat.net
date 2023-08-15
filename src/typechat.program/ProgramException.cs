// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public class ProgramException : Exception
{
    public enum ErrorCode
    {
        InvalidProgram,
        TypeNotSupported,
        TypeMistmatch,
        InvalidResultRef,
        ArgCountMismatch,
        FunctionNotFound,
        ConversionToJsonNotSupported
    }

    ErrorCode _errorCode;

    public ProgramException(ErrorCode code, string? message = null, Exception? inner = null)
        : base(message, inner)
    {
        _errorCode = code;
    }

    public ErrorCode Code => _errorCode;

    internal static void ThrowTypeMismatch(JsonValueKind expected, JsonValueKind actual)
    {
        throw new ProgramException(ProgramException.ErrorCode.TypeMistmatch, $"Type mismatch. Expected {expected}, Actual {actual}");
    }
    internal static void ThrowTypeMismatch(Type expected, Type actual)
    {
        throw new ProgramException(ProgramException.ErrorCode.TypeMistmatch, $"Type mismatch. Expected {expected}, Actual {actual}");
    }
    internal static void ThrowTypeMismatch(string name, Type expected, Type actual)
    {
        throw new ProgramException(ProgramException.ErrorCode.TypeMistmatch, $"Function {name} Type mismatch. Expected {expected}, Actual {actual}");
    }
    internal static void ThrowInvalidResultRef(int refId)
    {
        throw new ProgramException(ProgramException.ErrorCode.InvalidResultRef, $"{refId} is not a valid ResultReference");
    }
    internal static void ThrowInvalidResultRef(int resultRef, int maxResults)
    {
        throw new ProgramException(ProgramException.ErrorCode.InvalidResultRef, $"Referencing {resultRef} from {maxResults} results");
    }
    internal static void ThrowFunctionNotFound(string name)
    {
        throw new ProgramException(ProgramException.ErrorCode.FunctionNotFound, $"Function {name} not found");
    }
    internal static void ThrowVariableNotFound(string name)
    {
        throw new ProgramException(ProgramException.ErrorCode.FunctionNotFound, $"Variable {name} not found");
    }
    internal static void ThrowArgCountMismatch(string name, int expectedCount, int actualCount)
    {
        throw new ProgramException(ProgramException.ErrorCode.ArgCountMismatch, $"Function {name} Arg Count: Expected {expectedCount}, Got {actualCount}");
    }
    internal static void ThrowArgCountMismatch(FunctionCall call, int expectedCount, int actualCount)
    {
        string json = call.Source.ToString();
        string message = $"Function {call.Name} Arg Count: Expected {expectedCount}, Got {actualCount}\n\n{json}";
        throw new ProgramException(ProgramException.ErrorCode.ArgCountMismatch, message);
    }
}
