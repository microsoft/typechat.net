// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

public class TypeValidator<T> : IJsonTypeValidator<T>
{
    TypescriptSchema _schema;
    JsonSerializerTypeValidator<T> _jsonValidator;

    public TypeValidator(IVocabCollection? knownVocabs = null)
        : this(TypescriptExporter.GenerateSchema(typeof(T), knownVocabs))
    {
    }

    public TypeValidator(TypescriptSchema schema)
    {
        ArgumentNullException.ThrowIfNull(schema, nameof(schema));
        _schema = schema;
        _jsonValidator = new JsonSerializerTypeValidator<T>(_schema);
        if (_schema.HasVocabs)
        {
            _jsonValidator.Options.Converters.Add(new VocabStringJsonConvertor(_schema.Vocabs));
        }
    }

    public TypeSchema Schema => _schema;

    public Result<T> Validate(string json)
    {
        // Validate the raw json first
        Result<T> result = _jsonValidator.Validate(json);
        // Now do some constraints checking
        if (result.Success &&
            result.Value is IConstraintValidatable validatable)
        {
            string constraintsErrors = CheckConstraints(validatable);
            if (!string.IsNullOrEmpty(constraintsErrors))
            {
                // Constraints checks failed
                result = Result<T>.Error(constraintsErrors);
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
