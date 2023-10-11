// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

internal static class ListExtensions
{
    public static bool IsNullOrEmpty<T>(this IList<T> list)
    {
        return list == null || list.Count == 0;
    }
}
