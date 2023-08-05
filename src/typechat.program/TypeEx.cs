// Copyright (c) Microsoft. All rights reserved.
namespace Microsoft.TypeChat;

internal static class TypeEx
{
    internal static (bool, JsonValueKind) IsType(this JsonValueKind kind, Type type)
    {
        if (type.IsNumber())
        {
            return Compare(kind, JsonValueKind.Number);
        }
        if (type.IsString())
        {
            return Compare(kind, JsonValueKind.String);
        }
        if (type.IsBoolean())
        {
            return (kind == JsonValueKind.True) ?
                   (true, JsonValueKind.True) :
                   Compare(kind, JsonValueKind.False);
        }
        if (type.IsArray)
        {
            return Compare(kind, JsonValueKind.Array);
        }
        ProgramException.ThrowUnsupported(type);
        return Compare(kind, JsonValueKind.Undefined);
    }

    static (bool, JsonValueKind) Compare(JsonValueKind x, JsonValueKind expected)
    {
        return (x == expected, expected);
    }
}
