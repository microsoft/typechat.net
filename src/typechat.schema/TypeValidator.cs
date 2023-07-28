// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

public class TypeValidator<T> : IJsonTypeValidator<T>
{
    JsonSerializerTypeValidator<T> _jsonValidator;
    IVocabCollection? _vocabs;

    public TypeValidator(TypeSchema schema, IVocabCollection? vocabs)
    {
        _jsonValidator = new JsonSerializerTypeValidator<T>(schema);
        _vocabs = vocabs;
    }

    public TypeSchema Schema => _jsonValidator.Schema;

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
        ConstraintCheckContext context = new ConstraintCheckContext(errors, _vocabs);
        obj.ValidateConstraints(context);
        return errors.ToString();
    }
}
