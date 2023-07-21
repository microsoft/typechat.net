// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public class TypeSchema
{
    Type _type;
    Schema _schema;

    public TypeSchema(Type type, string schemaText)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));
        _schema = new Schema(schemaText);
        _type = type;
    }

    public TypeSchema(string name, Schema schema)
    {
        _type = Type.GetType(name);
        _schema = schema;
    }

    public Type Type => _type;
    public string TypeName => _type.Name;

    [JsonPropertyName("name")]
    public string TypeFullName => _type.FullName;

    [JsonPropertyName("schema")]
    public Schema Schema => _schema;
}
