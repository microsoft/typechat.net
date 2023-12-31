﻿// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// Place this attribute on properties to recursively validate child objects
/// </summary>
public class ValidateObjectAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        return ConstraintsValidator.Default.ValidateConstraints(value);
    }
}
