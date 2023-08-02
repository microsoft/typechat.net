// Copyright (c) Microsoft. All rights reserved.

using System.Runtime.InteropServices;

namespace Microsoft.TypeChat;

public struct AnyValue
{
    public readonly static AnyValue Undefined = new AnyValue();
    public readonly static AnyValue EmptyArray = new AnyValue[0];

    JsonValueKind _type;
    double _number;
    object? _obj;

    public AnyValue()
    {
        _type = JsonValueKind.Undefined;
        _number = 0;
        _obj = null;
    }

    public AnyValue(double number)
    {
        _type = JsonValueKind.Number;
        _number = number;
        _obj = null;
    }

    public AnyValue(string value)
    {
        _type = JsonValueKind.String;
        _number = 0;
        _obj = value;
    }

    public AnyValue(AnyValue[] values)
    {
        _type = JsonValueKind.Array;
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
            switch(_type)
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

    public AnyValue[] Array
    {
        get
        {
            if (_type != JsonValueKind.Array)
            {
                Throw(JsonValueKind.Array);
            }
            return _obj as AnyValue[];
        }
    }

    public void Clear()
    {
        _obj = null;
        _number = 0;
    }

    void Throw(JsonValueKind expected)
    {
        throw new ProgramException(ProgramException.ErrorCode.TypeMistmatch, $"Expected {expected}, Actual {_type}");
    }

    public static implicit operator AnyValue(double number)
    {
        return new AnyValue(number);
    }
    public static implicit operator AnyValue(string value)
    {
        return new AnyValue(value);
    }
    public static implicit operator AnyValue(AnyValue[] values)
    {
        return new AnyValue(values);
    }
}
