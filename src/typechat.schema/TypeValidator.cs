// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

public class TypeValidator<T> : IJsonTypeValidator<T>
{
    TypescriptSchema _schema;
    JsonSerializerTypeValidator<T> _jsonValidator;

    public TypeValidator(TypescriptSchema schema)
    {
        ArgumentNullException.ThrowIfNull(schema, nameof(schema));
        _schema = schema;
        _jsonValidator = new JsonSerializerTypeValidator<T>(_schema);
    }

    public TypeSchema Schema => _schema;

    public ValidationResult<T> Validate(string json)
    {
        // Validate the raw json first
        ValidationResult<T> result = _jsonValidator.Validate(json);
        // Now do some constraints checking
        if (result.Success &&
            result.Value is IConstraintValidatable validatable)
        {
            string constraintsErrors = CheckConstraints(validatable);
            if (!string.IsNullOrEmpty(constraintsErrors))
            {
                // Constraints checks failed
                result = ValidationResult<T>.Error(constraintsErrors);
            }
        }
        return result;
    }

    string CheckConstraints(IConstraintValidatable obj)
    {
        using StringWriter errors = new StringWriter();
        ConstraintCheckContext context = new ConstraintCheckContext(errors, _schema.Vocabs);
        obj.ValidateConstraints(context);
        return errors.ToString();
    }
}
