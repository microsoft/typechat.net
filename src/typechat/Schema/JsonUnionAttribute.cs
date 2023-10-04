// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

/// <summary>
/// Lets you specifiy a union of Types, like Typescript
/// </summary>
public class JsonUnionAttribute : JsonConverterAttribute
{
    UnionType _type;

    public JsonUnionAttribute(params Type[] types)
    {
        _type = new UnionType(types);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert)
    {
        if (_type.IsSupported(typeToConvert))
        {
            return new JsonUnionConverter(_type);
        }
        return null;
    }
}

internal class JsonUnionConverter : JsonConverter<object>
{
    UnionType _type;

    public JsonUnionConverter(UnionType type)
    {
        ArgumentVerify.ThrowIfNull(type, nameof(type));
        _type = type;
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

internal struct UnionTypeDef
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

internal class UnionType
{
    public const string TypeDiscriminatorPropertyName = "$type";

    UnionTypeDef[] _types;
    string _allTypes;

    public UnionType(Type[] types)
    {
        ArgumentVerify.ThrowIfNullOrEmpty(types, nameof(types));
        _types = new UnionTypeDef[types.Length];
        for (int i = 0; i < types.Length; ++i)
        {
            _types[i] = new UnionTypeDef(types[i]);
        }
    }

    public UnionType(UnionTypeDef[] types)
    {
        ArgumentVerify.ThrowIfNullOrEmpty(types, nameof(types));
        _types = types;
        _allTypes = AllTypeNames();
    }

    public UnionTypeDef[] Types => _types;

    public bool IsSupported(Type type) => (IndexOfType(type) >= 0);

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
        int i = IndexOfType(type);
        if (i >= 0)
        {
            return _types[i];
        }
        throw new JsonException($"{type.Name} is not recognized. Permitted values: {_allTypes}");
    }

    int IndexOfType(Type type)
    {
        for (int i = 0; i < _types.Length; ++i)
        {
            if (_types[i].Type == type)
            {
                return i;
            }
        }
        return -1;
    }

    UnionTypeDef ResolveType(string typeDiscriminator)
    {
        for (int i = 0; i < _types.Length; ++i)
        {
            if (_types[i].TypeDiscriminator == typeDiscriminator)
            {
                return _types[i];
            }
        }
        throw new JsonException($"{typeDiscriminator} is not recognized. Permitted values: {_allTypes}");
    }

    string AllTypeNames()
    {
        var names = (from typeDef in _types select typeDef.TypeDiscriminator);
        return string.Join(",", names);
    }
}
