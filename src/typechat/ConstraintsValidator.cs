// Copyright (c) Microsoft. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace Microsoft.TypeChat;

/// <summary>
/// Once Json Translator has a valid value of type T, applies additional
/// constraints validation and business rules
/// </summary>
public interface IConstraintsValidator<T>
{
    /// <summary>
    /// Validate the value T
    /// </summary>
    /// <param name="value">check constraints for this value</param>
    /// <returns>result of the validation</returns>
    Result<T> Validate(T value);
}

/// <summary>
/// Validation support using infrastructure from System.Component.DataAnnotations
/// </summary>
public class ConstraintsValidator<T> : IConstraintsValidator<T>
{
    public ConstraintsValidator() { }

    /// <summary>
    /// Validate the given value
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
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
