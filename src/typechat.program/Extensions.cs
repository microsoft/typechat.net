// Copyright (c) Microsoft. All rights reserved.
namespace Microsoft.TypeChat;

internal static class Extensions
{
    internal static bool IsAsync(this ParameterInfo returnType)
    {
        return (returnType.ParameterType.IsAssignableTo(typeof(Task)));
    }
}
