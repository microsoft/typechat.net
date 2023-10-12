// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

internal static class ListEx
{
    internal static bool IsNullOrEmpty<T>(this IList<T> list)
    {
        return (list == null || list.Count == 0);
    }

    internal static void Trim<T>(this List<T> list, int trimCount)
    {
        if (trimCount > list.Count)
        {
            list.Clear();
        }
        else
        {
            list.RemoveRange(list.Count - trimCount, trimCount);
        }
    }
}
