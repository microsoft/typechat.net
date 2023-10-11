// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Validation;

/// <summary>
/// Validates constrains specified using attributes defined in System.ComponentModel.DataAnnotations
/// </summary>
public class ConstraintsValidator
{
    public static readonly ConstraintsValidator Default = new ConstraintsValidator();

    public ConstraintsValidator() { }

    /// <summary>
    /// Validate the given value
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public ValidationResult ValidateConstraints(object value)
    {
        // Future: Pool these
        ValidationContext validationContext = new ValidationContext(value);
        List<ValidationResult> validationResults = new List<ValidationResult>();
        if (Validator.TryValidateObject(value, validationContext, validationResults, true))
        {
            return ValidationResult.Success;
        }

        string errorMessage = ToErrorString(validationResults);
        return new ValidationResult(errorMessage);
    }

    string ToErrorString(List<ValidationResult> validationResults)
    {
        // Future: pool these
        StringBuilder sb = new StringBuilder();
        foreach (var result in validationResults)
        {
            sb.AppendLine(result.ErrorMessage);
        }
        return sb.ToString();
    }
}

/// <summary>
/// Validation support using infrastructure from System.Component.DataAnnotations
/// </summary>
public class ConstraintsValidator<T> : ConstraintsValidator, IConstraintsValidator<T>
{
    public ConstraintsValidator() { }

    /// <summary>
    /// Validate the given value
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public Result<T> Validate(T value)
    {
        ValidationResult result = Default.ValidateConstraints(value);
        if (result == ValidationResult.Success)
        {
            return new Result<T>(value);
        }

        return Result<T>.Error(result.ErrorMessage);
    }

    string ToErrorString(List<ValidationResult> validationResults)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var result in validationResults)
        {
            sb.AppendLine(result.ErrorMessage);
        }
        return sb.ToString();
    }
}
