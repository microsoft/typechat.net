﻿// Copyright (c) Microsoft. All rights reserved.

using Microsoft.TypeChat.Schema;

namespace Microsoft.TypeChat;

/// <summary>
/// A TypeValidator that:
/// - Defines Schema using Typescript. Schema can be auto-generated for you
/// - Validates and converts Json responses to T using the Json Serializer
/// </summary>
/// <typeparam name="T"></typeparam>
public class TypeValidator<T> : IJsonTypeValidator<T>
{
    TypescriptSchema _schema;
    JsonSerializerTypeValidator<T> _jsonValidator;

    /// <summary>
    /// Create a new TypeValidator.
    /// Automatically generates a Typescript schema for the given type T. Auto-generation handles
    /// common scenarios, but you can also define schema manually for advanced use cases. 
    /// </summary>
    /// <param name="knownVocabs">Known vocabularies that attributes like JsonVocab bind to</param>
    public TypeValidator(IVocabCollection? knownVocabs = null)
        : this(TypescriptExporter.GenerateSchema(typeof(T), knownVocabs))
    {
    }

    /// <summary>
    /// Create a TypeValidator that uses the given Typescript schema
    /// </summary>
    /// <param name="schema">Typescript schema to use</param>
    public TypeValidator(TypescriptSchema schema)
    {
        ArgumentVerify.ThrowIfNull(schema, nameof(schema));
        _schema = schema;
        _jsonValidator = new JsonSerializerTypeValidator<T>(_schema);
        if (_schema.HasVocabs)
        {
            _jsonValidator.Options.Converters.Add(new VocabStringJsonConvertor(_schema.Vocabs));
        }
    }

    /// <summary>
    /// Schema used by this validator
    /// </summary>
    public TypeSchema Schema => _schema;

    /// <summary>
    /// Validate the given Json string.
    /// If Result.Success: includes a value of type T
    /// If Result.False: includes a diagnostic message
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public Result<T> Validate(string json)
    {
        return _jsonValidator.Validate(json);
    }
}
