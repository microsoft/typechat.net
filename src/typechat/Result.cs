// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public class Result<T>
{
    public Result(T value, string? message = null)
    {
        Success = true;
        Value = value;
        Message = message;
    }

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

    public T Value { get; private set; }
    public bool Success { get; private set; }
    public string? Message { get; set; }

    public static implicit operator Result<T>(T value)
    {
        return new Result<T>(value);
    }
    public static implicit operator T(Result<T> result)
    {
        return result.Value;
    }
    public static Result<T> Error(string message)
    {
        return new Result<T>(false, message);
    }
    public static Result<T> Error(T value, string message)
    {
        var result = new Result<T>(false, message);
        result.Value = value;
        return result;
    }
}
