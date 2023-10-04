// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;
/// <summary>
/// Lets you specifiy a union of Types, like Typescript
/// </summary>
public class JsonUnionAttribute : JsonConverterAttribute
{
    Type[] _types;

    public JsonUnionAttribute(params Type[] types)
    {
        ArgumentVerify.ThrowIfNullOrEmpty(types, nameof(types));
        _types = types;
    }

    public override JsonConverter? CreateConverter(Type typeToConvert)
    {
        return new JsonUnionConverter(_types);
    }
}

public class JsonUnionConverter : JsonConverter<object>
{
    UnionType _type;

    public JsonUnionConverter(Type[] types)
    {
        UnionTypeDef[] typeDef = new UnionTypeDef[types.Length];
        for (int i = 0; i < types.Length; ++i)
        {
            typeDef[i] = new UnionTypeDef(types[i]);
        }
        _type = new UnionType(typeDef);
    }

    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return _type.Deserialize(ref reader, options);
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}

public struct UnionTypeDef
{
    Type _type;
    string _typeDiscriminator;

    public UnionTypeDef(Type type)
        : this(type, type.Name)
    {
    }

    public UnionTypeDef(Type type, string typeDiscriminator)
    {
        _type = type;
        _typeDiscriminator = typeDiscriminator;
    }

    public Type Type => _type;
    public string TypeDiscriminator => _typeDiscriminator;
}

public class UnionType
{
    public const string TypeDiscriminatorPropertyName = "$type";

    UnionTypeDef[] _types;
    string _allTypes;

    public UnionType(UnionTypeDef[] types)
    {
        ArgumentVerify.ThrowIfNullOrEmpty(types, nameof(types));
        _types = types;
        _allTypes = AllTypeNames();
    }

    public UnionTypeDef[] Types => _types;

    public object? Deserialize(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        string typeName = reader.GetStringProperty(TypeDiscriminatorPropertyName);
        UnionTypeDef typeDef = ResolveType(typeName);
        return JsonSerializer.Deserialize(ref reader, typeDef.Type, options);
    }

    public void Serialize(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        UnionTypeDef typeDef = ResolveType(value.GetType());
        writer.WritePropertyName(TypeDiscriminatorPropertyName);
        writer.WriteStringValue(typeDef.TypeDiscriminator);
        JsonSerializer.Serialize(writer, value, options);
    }

    UnionTypeDef ResolveType(Type type)
    {
        for (int i = 0; i < _types.Length; ++i)
        {
            if (_types[i].Type == type)
            {
                return _types[i];
            }
        }
        throw new JsonException($"{type.Name} is not recognized. Permitted values: {_allTypes}");
    }

    UnionTypeDef ResolveType(string typeName)
    {
        for (int i = 0; i < _types.Length; ++i)
        {
            if (_types[i].TypeDiscriminator == typeName)
            {
                return _types[i];
            }
        }
        throw new JsonException($"{typeName} is not recognized. Permitted values: {_allTypes}");
    }

    string AllTypeNames()
    {
        var names = (from typeDef in _types select typeDef.TypeDiscriminator);
        return string.Join(",", names);
    }
}
