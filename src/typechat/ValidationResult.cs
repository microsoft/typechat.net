// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public class ValidationResult<T>
{
    public ValidationResult(T value)
    {
        Success = true;
        Value = value;
    }

    public ValidationResult(ValidationResult<object?> value)
    {
        Success = value.Success;
        Value = (T)value.Value;
        Message = value.Message;
    }

    internal ValidationResult(bool success, string? message)
    {
        Success = success;
        Message = message;
        Value = default;
    }

    public T Value { get; private set; }
    public bool Success { get; private set; }
    public string? Message { get; set; }

    public static implicit operator ValidationResult<T>(T value)
    {
        return new ValidationResult<T>(value);
    }
    public static implicit operator T(ValidationResult<T> result)
    {
        if (!result.Success)
        {
            throw new InvalidOperationException(result.Message);
        }
        return result.Value;
    }
    public static ValidationResult<T> Error(string message)
    {
        return new ValidationResult<T>(false, message);
    }
}
