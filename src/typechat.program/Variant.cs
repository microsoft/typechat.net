// Copyright (c) Microsoft. All rights reserved.

using System.Runtime.InteropServices;

namespace Microsoft.TypeChat;

public enum VariantType : byte
{
    Null,
    String,
    Bool,
    Int,
    Long,
    Double,
}

public struct Variant
{
    VariantType _type;
    object _obj;
    VariantValue _value;

    public Variant()
    {
        _type = VariantType.Null;
        _obj = null;
    }

    public Variant(string value)
    {
        _type = VariantType.String;
        _obj = value;
    }

    public Variant(bool value)
    {
        _type = VariantType.Bool;
        _value = new VariantValue { Bool = value };
    }

    public Variant(int value)
    {
        _type = VariantType.Int;
        _value = new VariantValue { Int = value };
    }

    public Variant(double value)
    {
        _type = VariantType.Double;
        _value = new VariantValue { Double = value };
    }
}

/// <summary>
/// 8 bytes max
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public struct VariantValue
{
    [FieldOffset(0)]
    public bool Bool;
    [FieldOffset(0)]
    public int Int;
    [FieldOffset(0)]
    public long Long;
    [FieldOffset(0)]
    public double Double;

    public void Clear()
    {
        Long = 0;
    }
}
