// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

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
        return _jsonValidator.Validate(json);
    }
}
