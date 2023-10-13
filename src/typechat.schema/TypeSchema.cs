// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

/// <summary>
/// The Schema for a Type
/// </summary>
public class TypeSchema
{
    /// <summary>
    /// Create a new TypeSchema
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="schema">schema for the type</param>
    /// <exception cref="ArgumentNullException"></exception>
    public TypeSchema(Type type, SchemaText schema)
    {
        ArgumentVerify.ThrowIfNull(type, nameof(type));
        Schema = schema;
        Type = type;
    }

    /// <summary>
    /// Create TypeSchema
    /// </summary>
    /// <param name="name">Type name</param>
    /// <param name="schema">Schema text for type</param>
    public TypeSchema(string name, SchemaText schema)
        : this(System.Type.GetType(name), schema)
    {
    }

    [JsonIgnore]
    public Type Type { get; }

    /// <summary>
    /// Name of the type
    /// </summary>
    [JsonIgnore]
    public string TypeName => Type.Name;

    /// <summary>
    /// Full name for the type
    /// </summary>
    [JsonPropertyName("name")]
    public string TypeFullName => Type.FullName;

    /// <summary>
    /// Schema for the Type
    /// </summary>
    [JsonPropertyName("schema")]
    public SchemaText Schema { get; }
}
