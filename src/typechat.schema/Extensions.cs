// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

internal static class Extensions
{
    internal static bool IsNullOrEmpty(this Array array)
    {
        return (array == null || array.Length == 0);
    }
}
