// Copyright (c) Microsoft. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace Microsoft.TypeChat;

public interface IConstraintsValidator<T>
{
    Result<T> Validate(T value);
}

/// <summary>
/// Validation support using infrastructure from System.Component.DataAnnotations
/// </summary>
public class ConstraintsValidator<T> : IConstraintsValidator<T>
{
    public ConstraintsValidator() { }

    public Result<T> Validate(T value)
    {
        // Future: Pool these
        ValidationContext validationContext = new ValidationContext(value);
        List<ValidationResult> validationResults = new List<ValidationResult>();
        if (Validator.TryValidateObject(value, validationContext, validationResults))
        {
            return value;
        }

        string errorMessage = ToErrorString(validationResults);
        return Result<T>.Error(value, errorMessage);
    }

    string ToErrorString(List<ValidationResult> validationResults)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var result in validationResults)
        {
            sb.AppendLine("Errors in the following: ");
            sb.AppendLine(string.Join(",", result.MemberNames));
            sb.AppendLine(result.ErrorMessage);
        }
        return sb.ToString();
    }
}
