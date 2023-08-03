// Copyright (c) Microsoft. All rights reserved.

using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.TypeChat;

/// <summary>
/// Represents any Json Value incluing undefined and null.
/// Use JsonValueKind to get the Type
/// </summary>
public struct AnyJsonValue
{
    public readonly static AnyJsonValue Undefined = new AnyJsonValue();
    public readonly static AnyJsonValue[] EmptyArray = new AnyJsonValue[0];

    JsonValueKind _type;
    double _number;
    object? _obj;

    public AnyJsonValue()
    {
        _type = JsonValueKind.Undefined;
        _number = 0;
        _obj = null;
    }

    public AnyJsonValue(double number)
    {
        _type = JsonValueKind.Number;
        _number = number;
        _obj = null;
    }

    public AnyJsonValue(string value)
    {
        ArgumentNullException.ThrowIfNull(value, nameof(value));
        _type = JsonValueKind.String;
        _number = 0;
        _obj = value;
    }

    public AnyJsonValue(AnyJsonValue[] values)
    {
        ArgumentNullException.ThrowIfNull(values, nameof(values));
        _type = JsonValueKind.Array;
        _number = 0;
        _obj = values;
    }

    public AnyJsonValue(Dictionary<string, AnyJsonValue> values)
    {
        ArgumentNullException.ThrowIfNull(values, nameof(values));
        _type = JsonValueKind.Object;
        _number = 0;
        _obj = values;
    }

    public JsonValueKind Type => _type;

    public bool IsUndefined => _type == JsonValueKind.Undefined;

    public object Null
    {
        get
        {
            if (_type != JsonValueKind.Null)
            {
                Throw(JsonValueKind.Null);
            }
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
                    throw new ProgramException(ProgramException.ErrorCode.TypeMistmatch, $"Expected boolean");
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
            if (_type != JsonValueKind.Number)
            {
                Throw(JsonValueKind.Number);
            }
            return _number;
        }
    }

    public string String
    {
        get
        {
            if (_type != JsonValueKind.String)
            {
                Throw(JsonValueKind.String);
            }
            return _obj as string;
        }
    }

    public AnyJsonValue[] Array
    {
        get
        {
            if (_type != JsonValueKind.Array)
            {
                Throw(JsonValueKind.Array);
            }
            return _obj as AnyJsonValue[];
        }
    }

    public Dictionary<string, AnyJsonValue> Object
    {
        get
        {
            if (_type != JsonValueKind.Object)
            {
                Throw(JsonValueKind.Object);
            }
            return _obj as Dictionary<string, AnyJsonValue>;
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


    void Throw(JsonValueKind expected)
    {
        throw new ProgramException(ProgramException.ErrorCode.TypeMistmatch, $"Expected {expected}, Actual {_type}");
    }

    public static implicit operator AnyJsonValue(double number)
    {
        return new AnyJsonValue(number);
    }
    public static implicit operator double(AnyJsonValue value)
    {
        return value.Number;
    }
    public static implicit operator string(AnyJsonValue value)
    {
        return value.String;
    }
    public static implicit operator AnyJsonValue(string value)
    {
        return new AnyJsonValue(value);
    }
    public static implicit operator AnyJsonValue(AnyJsonValue[] values)
    {
        return new AnyJsonValue(values);
    }
    public static implicit operator AnyJsonValue(Dictionary<string, AnyJsonValue> values)
    {
        return new AnyJsonValue(values);
    }
}
