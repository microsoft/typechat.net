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
        Value = value.Success || value.Value is T ? (T)value.Value : default;
        Message = value.Message;
        Failure = value.Failure;
        Info = value.Info;
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

    /// <summary>
    /// Optional metadata (token usage, finish reason, repair count, and the raw response when
    /// available) about the final completion that produced this result. May be null.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public CompletionInfo? Info { get; set; }

    /// <summary>
    /// Optional unvalidated output associated with a failed result. Value remains the typed
    /// result; use Failure.Value for rejected data that does not conform to T.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public FailureInfo? Failure { get; private set; }

    /// <summary>
    /// Returns a shallow copy of this failed result, retaining existing failure fields and
    /// filling unavailable fields from the supplied context. Custom result metadata is preserved.
    /// </summary>
    /// <param name="failure">Fallback failure context.</param>
    /// <returns>A failed result with the combined context.</returns>
    public Result<T> WithFailure(FailureInfo failure)
    {
        ArgumentVerify.ThrowIfNull(failure, nameof(failure));
        if (Success)
        {
            throw new InvalidOperationException("Failure context cannot be added to a successful result.");
        }
        var result = (Result<T>)MemberwiseClone();
        result.Failure = Failure is null ? failure.WithFallback(null) : Failure.WithFallback(failure);
        return result;
    }

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
        var result = new Result<T>(false, message);
        result.Value = value;
        result.Failure = new FailureInfo { Value = value };
        return result;
    }
}
