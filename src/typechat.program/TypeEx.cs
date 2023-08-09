// Copyright (c) Microsoft. All rights reserved.
namespace Microsoft.TypeChat;

internal static class TypeEx
{
    static (bool, JsonValueKind) Compare(JsonValueKind x, JsonValueKind expected)
    {
        return (x == expected, expected);
    }
}
