// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// Result of an operation that returns T.
/// If result is Success, then includes a Value of type T
/// If result is Failure, includes an optional diagnostic message
/// </summary>
/// <typeparam name="T">Returned type</typeparam>
public class Result<T>
{
    /// <summary>
    /// Create a new result with the given value and diagnostic message
    /// </summary>
    /// <param name="value">result value</param>
    /// <param name="message">diagnostic message</param>
    public Result(T value, string? message = null)
    {
        Success = true;
        Value = value;
        Message = message;
    }

    /// <summary>
    /// Clones the given result
    /// </summary>
    /// <param name="value">result</param>
    public Result(Result<object?> value)
    {
        Success = value.Success;
        Value = (T)value.Value;
        Message = value.Message;
    }

    internal Result(bool success, string? message)
    {
        Success = success;
        Message = message;
        Value = default;
    }

    /// <summary>
    /// Success or failure
    /// </summary>
    public bool Success { get; private set; }

    /// <summary>
    /// Result value.
    /// </summary>
    public T Value { get; private set; }

    /// <summary>
    /// Diagnostic message in case of failure
    /// </summary>
    public string? Message { get; set; }

    public static implicit operator Result<T>(T value)
    {
        return new Result<T>(value);
    }
    public static implicit operator T(Result<T> result)
    {
        return result.Value;
    }

    /// <summary>
    /// Create an error result
    /// </summary>
    /// <param name="message">diagnostic message</param>
    /// <returns>Result</returns>
    public static Result<T> Error(string message)
    {
        return new Result<T>(false, message);
    }

    /// <summary>
    /// Create an Error result
    /// </summary>
    /// <param name="value">a value, if available</param>
    /// <param name="message"></param>
    /// <returns></returns>
    public static Result<T> Error(T value, string message)
    {
        var result = new Result<T>(false, message)
        {
            Value = value
        };
        return result;
    }
}
