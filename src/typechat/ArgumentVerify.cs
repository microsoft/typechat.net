// Copyright (c) Microsoft. All rights reserved.
using System.Runtime.CompilerServices;

namespace Microsoft.TypeChat;

/// <summary>
/// Used by Typechat libraries to do argument validation in a .NET framework agnostic way
/// </summary>
public static class ArgumentVerify
{
    public static void Throw(string message)
    {
        throw new ArgumentException(message);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfNull(object argument, string paramName)
    {
        if (argument == null)
        {
            throw new ArgumentNullException(paramName);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfNullOrEmpty(string argument, string paramName)
    {
        ThrowIfNull(argument, paramName);
        if (string.IsNullOrEmpty(argument))
        {
            throw new ArgumentException("The value cannot be an empty string.", paramName);
        }
    }
}
