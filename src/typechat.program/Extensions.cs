// Copyright (c) Microsoft. All rights reserved.
namespace Microsoft.TypeChat;

internal static class Extensions
{
    internal static bool IsAsync(this ParameterInfo returnType)
    {
        return (returnType.ParameterType.IsAssignableTo(typeof(Task)));
    }

    internal static string Stringify<T>(this T value)
    {
        return JsonSerializer.Serialize<T>(value, JsonProgramConvertor.Options);
    }

    internal static bool IsNullOrEmpty(this Array array)
    {
        return (array == null || array.Length == 0);
    }
}
