// Copyright (c) Microsoft. All rights reserved.
namespace Microsoft.TypeChat;

internal static class Extensions
{
    internal static bool IsAsync(this ParameterInfo returnType)
    {
        return (returnType.ParameterType.IsAssignableTo(typeof(Task)));
    }

    internal static bool IsJsonObject(this Type type)
    {
        return type.IsAssignableFrom(typeof(JsonObject));
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
        return (otherType.IsAssignableTo(expectedType));
    }

    internal static string Stringify<T>(this T value)
    {
        return JsonSerializer.Serialize<T>(value, JsonProgramConvertor.Options);
    }
}
