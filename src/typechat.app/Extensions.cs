// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public static class Extensions
{
    public static string? GetOrNull(this string[] args, int index)
    {
        if (args == null ||
            index >= args.Length)
        {
            return null;
        }
        return args[index];
    }
}
