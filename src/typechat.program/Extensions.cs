// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

internal static class Extensions
{
    internal static bool IsAsync(this ParameterInfo returnType)
    {
#if NET7_0_OR_GREATER
        return (returnType.ParameterType.IsAssignableTo(typeof(Task)));
#else
        return (typeof(Task)).IsAssignableFrom(returnType.ParameterType);
#endif
    }

    internal static bool IsJsonObject(this Type type)
    {
#if NET7_0_OR_GREATER
        return type.IsAssignableTo(typeof(JsonObject));
#else
        return typeof(JsonObject).IsAssignableFrom(type);
#endif
    }

    /// <summary>
    /// Two types can be implicitly the same due to (a) equality (b) casting
    /// </summary>
    internal static bool IsConvertibleFrom(this ParameterInfo param, Type fromType)
    {
        return (param.ParameterType == fromType ||
               param.ParameterType.IsPrimitive && fromType.IsPrimitive);
    }

    internal static bool CanBeDeserialized(this ParameterInfo param)
    {
        return (!param.ParameterType.IsPrimitive &&
                !param.ParameterType.IsString());
    }

    internal static bool IsMatchingType(this ParameterInfo param, Type otherType)
    {
        Type expectedType = param.ParameterType;
        if (expectedType.IsArray)
        {
            if (!otherType.IsArray)
            {
                return false;
            }
            return (expectedType.GetElementType() == otherType.GetElementType());
        }
#if NET7_0_OR_GREATER
        return (otherType.IsAssignableTo(expectedType));
#else
        return (expectedType.IsAssignableFrom(otherType));
#endif
    }

    internal static string Stringify<T>(this T value)
    {
        return JsonSerializer.Serialize<T>(value, JsonProgramConvertor.Options);
    }
}
