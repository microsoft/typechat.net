// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public interface IJsonTypeValidator
{
    ValidationResult<object?> Validate(TypeSchema schema, string json);
}

public interface IJsonTypeValidator<T>
{
    TypeSchema Schema { get; }
    ValidationResult<T> Validate(string json);
}
