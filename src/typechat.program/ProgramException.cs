// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// Exception thrown when there is any issue with a program
/// </summary>
public class ProgramException : Exception
{
    /// <summary>
    /// Exception error codes
    /// </summary>
    public enum ErrorCode
    {
        // Program JSON does not contain a valid program
        InvalidProgramJson,
        JsonValueTypeNotSupported,
        TypeMismatch,      // There was a type mismatch when attempting to call a function
        InvalidResultRef,   // Invalid Reference to a result returned by a program step
        ArgCountMismatch,   // 
        FunctionNotFound,   // No such function in the target Api
        UnknownExpression
    }

    private ErrorCode _errorCode;

    public ProgramException(ErrorCode code, string? message = null, Exception? inner = null)
        : base(message, inner)
    {
        _errorCode = code;
    }

    public ErrorCode Code => _errorCode;

    public static void ThrowFunctionNotFound(string name)
    {
        throw new ProgramException(ProgramException.ErrorCode.FunctionNotFound, $"@func {name} not found in API");
    }
    public static void ThrowArgCountMismatch(FunctionCall call, int expectedCount, int actualCount)
    {
        string json = call.Source.ToString();
        string message = $"@func {call.Name}: Wrong number of arguments passed. Expected {expectedCount}, Got {actualCount}\n\n{json}";
        throw new ProgramException(ProgramException.ErrorCode.ArgCountMismatch, message);
    }

    internal static void ThrowArgCountMismatch(string name, int expectedCount, int actualCount)
    {
        throw new ProgramException(ProgramException.ErrorCode.ArgCountMismatch, $"@func {name} Wrong number of arguments passed. Expected {expectedCount}, Got {actualCount}");
    }

    internal static void ThrowTypeMismatch(FunctionCall call, ParameterInfo param, Type actual)
    {
        ThrowTypeMismatch(call.Name, param, actual);
    }

    internal static void ThrowTypeMismatch(string name, ParameterInfo param, Type actual)
    {
        throw new ProgramException(
            ProgramException.ErrorCode.TypeMismatch,
            $"TypeMismatch: @func {name} @arg {param.Name}: Expected {param.ParameterType.Name}, Got {actual.Name}"
            );
    }

    internal static void ThrowInvalidResultRef(int refId)
    {
        throw new ProgramException(ProgramException.ErrorCode.InvalidResultRef, $"{refId} is not a valid ResultReference");
    }
    internal static void ThrowInvalidResultRef(int resultRef, int maxResults)
    {
        throw new ProgramException(ProgramException.ErrorCode.InvalidResultRef, $"Referencing @ref: {resultRef} that is not available yet. Only {maxResults} results available");
    }
    internal static void ThrowVariableNotFound(string name)
    {
        throw new ProgramException(ProgramException.ErrorCode.FunctionNotFound, $"Variable {name} not found");
    }
}
