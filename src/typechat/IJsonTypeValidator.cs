// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public interface IJsonTypeValidator
{
    Result<object?> Validate(TypeSchema schema, string json);
}

public interface IJsonTypeValidator<T>
{
    TypeSchema Schema { get; }
    Result<T> Validate(string json);
}
