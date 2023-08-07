// Copyright (c) Microsoft. All rights reserved.

using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Nodes;

namespace Microsoft.TypeChat;

public enum JsonObjectKind : byte
{
    NotObject = 0,
    Dictionary,
    JsonObject,
    Object
}

/// <summary>
/// Represents any Json Value incluing undefined and null.
/// Use JsonValueKind to get the Type
/// </summary>
public struct AnyJsonValue
{
    public readonly static AnyJsonValue Undefined = new AnyJsonValue();
    public readonly static AnyJsonValue[] EmptyArray = System.Array.Empty<AnyJsonValue>();

    JsonValueKind _type;
    JsonObjectKind _objType;
    double _number;
    object? _obj;

    public AnyJsonValue()
    {
        _type = JsonValueKind.Undefined;
        _objType = JsonObjectKind.NotObject;
        _number = double.NaN;
        _obj = null;
    }

    public AnyJsonValue(double number)
    {
        _type = JsonValueKind.Number;
        _objType = JsonObjectKind.NotObject;
        _number = number;
        _obj = null;
    }

    public AnyJsonValue(string value)
    {
        ArgumentNullException.ThrowIfNull(value, nameof(value));
        _type = JsonValueKind.String;
        _objType = JsonObjectKind.NotObject;
        _number = double.NaN;
        _obj = value;
    }

    public AnyJsonValue(bool value)
    {
        _type = value ? JsonValueKind.True : JsonValueKind.False;
        _objType = JsonObjectKind.NotObject;
        _number = double.NaN;
        _obj = value;
    }

    public AnyJsonValue(AnyJsonValue[] values)
    {
        ArgumentNullException.ThrowIfNull(values, nameof(values));
        _type = JsonValueKind.Array;
        _objType = JsonObjectKind.NotObject;
        _number = double.NaN;
        _obj = values;
    }

    public AnyJsonValue(Dictionary<string, AnyJsonValue> jsonObject)
    {
        ArgumentNullException.ThrowIfNull(jsonObject, nameof(jsonObject));
        _type = JsonValueKind.Object;
        _objType = JsonObjectKind.Dictionary;
        _number = double.NaN;
        _obj = jsonObject;
    }

    public AnyJsonValue(JsonObject obj)
    {
        _type = JsonValueKind.Object;
        _objType = JsonObjectKind.JsonObject;
        _number = double.NaN;
        _obj = obj;
    }

    public AnyJsonValue(object obj)
    {
        _type = JsonValueKind.Object;
        _objType = JsonObjectKind.Object;
        _number = double.NaN;
        _obj = obj;
    }

    AnyJsonValue(JsonValueKind kind)
    {
        _type = kind;
        _number = double.NaN;
        _obj = null;
    }

    public JsonValueKind Type => _type;
    public JsonObjectKind ObjectType => _objType;

    public bool IsUndefined => _type == JsonValueKind.Undefined;

    public object Null
    {
        get
        {
            CheckType(JsonValueKind.Null);
            return null;
        }
    }

    public bool Bool
    {
        get
        {
            switch (_type)
            {
                default:
                    ProgramException.ThrowTypeMismatch(JsonValueKind.True, JsonValueKind.False, _type);
                    return false;
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.True:
                    return true;
            }
        }
    }

    public double Number
    {
        get
        {
            CheckType(JsonValueKind.Number);
            return _number;
        }
    }

    public string String
    {
        get
        {
            CheckType(JsonValueKind.String);
            return _obj as string;
        }
    }

    public AnyJsonValue[] Array
    {
        get
        {
            CheckType(JsonValueKind.Array);
            return _obj as AnyJsonValue[];
        }
    }

    public Dictionary<string, AnyJsonValue> JsonDictionary
    {
        get
        {
            CheckType(JsonValueKind.Object);
            CheckObjectType(JsonObjectKind.Dictionary);
            return _obj as Dictionary<string, AnyJsonValue>;
        }
    }

    public object Object
    {
        get
        {
            CheckType(JsonValueKind.Object);
            CheckObjectType(JsonObjectKind.Object);
            return _obj;
        }
    }

    public JsonObject JsonObject
    {
        get
        {
            CheckType(JsonValueKind.Object);
            CheckObjectType(JsonObjectKind.JsonObject);
            return _obj as JsonObject;
        }
    }

    public void Clear()
    {
        _obj = null;
        _number = 0;
    }

    public override string ToString()
    {
        switch (_type)
        {
            default:
                break;
            case JsonValueKind.String:
                return String;
            case JsonValueKind.Number:
                return Number.ToString();
            case JsonValueKind.Undefined:
                return "undefined";
            case JsonValueKind.Null:
                return "null";
            case JsonValueKind.True:
                return "true";
            case JsonValueKind.False:
                return "false";
        }
        return base.ToString();
    }

    public object? ToObject(Type type)
    {
        if (type.IsNumber())
        {
            return Number;
        }
        if (type.IsString())
        {
            return String;
        }
        if (type.IsBoolean())
        {
            return Bool;
        }
        if (type.IsArray)
        {
            return ToObject(Array, type.GetElementType());
        }
        ProgramException.ThrowUnsupported(type);
        return null;
    }

    public JsonNode ToJsonNode()
    {
        switch(_type)
        {
            default:
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                return null;
            case JsonValueKind.Number:
                return Number;
            case JsonValueKind.String:
                return String;
            case JsonValueKind.True:
                return true;
            case JsonValueKind.False:
                return false;
            case JsonValueKind.Array:
                return new JsonArray(ToJsonNode(Array));
            case JsonValueKind.Object:
                throw new NotSupportedException();
        }
    }

    object?[] ToObject(AnyJsonValue[] jsonArray, Type type)
    {
        object?[] array = new object[jsonArray.Length];
        for (int i = 0; i < jsonArray.Length; ++i)
        {
            array[i] = jsonArray[i].ToObject(type);
        }
        return array;
    }

    JsonNode[] ToJsonNode(AnyJsonValue[] jsonArray)
    {
        JsonNode[] array = new JsonNode[jsonArray.Length];
        for (int i = 0; i < jsonArray.Length; ++i)
        {
            array[i] = jsonArray[i].ToJsonNode();
        }
        return array;
    }

    public static AnyJsonValue FromObject(Type type, object? obj)
    {
        if (type.IsNumber())
        {
            return new AnyJsonValue((double)obj);
        }
        if (type.IsString())
        {
            return new AnyJsonValue((string)obj);
        }
        if (type.IsBoolean())
        {
            return new AnyJsonValue((bool)obj);
        }

        ProgramException.ThrowUnsupported(type);
        return AnyJsonValue.Undefined;
    }

    void CheckType(JsonValueKind expected)
    {
        if (_type != expected)
        {
            Throw(expected);
        }
    }

    void CheckObjectType(JsonObjectKind expected)
    {
        if (_objType != expected)
        {
            Throw(expected);
        }
    }

    void Throw(JsonValueKind expected) => ProgramException.ThrowTypeMismatch(expected, _type);
    void Throw(JsonObjectKind expected)
    {
        throw new ProgramException(ProgramException.ErrorCode.TypeMistmatch, $"Expected {expected}, Actual {_objType}");
    }

    public static implicit operator AnyJsonValue(double number)
    {
        return new AnyJsonValue(number);
    }
    public static implicit operator AnyJsonValue(string value)
    {
        return new AnyJsonValue(value);
    }
    public static implicit operator AnyJsonValue(AnyJsonValue[] values)
    {
        return new AnyJsonValue(values);
    }
    public static implicit operator AnyJsonValue(JsonNode obj)
    {
        return new AnyJsonValue((JsonNode)obj);
    }
    public static implicit operator AnyJsonValue(Dictionary<string, AnyJsonValue> values)
    {
        return new AnyJsonValue(values);
    }
    public static implicit operator double(AnyJsonValue value)
    {
        return value.Number;
    }
    public static implicit operator string(AnyJsonValue value)
    {
        return value.String;
    }
    public static implicit operator JsonNode(AnyJsonValue value)
    {
        return value.ToJsonNode();
    }
}
