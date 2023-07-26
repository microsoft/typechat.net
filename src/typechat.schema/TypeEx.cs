// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

internal static class TypeEx
{
    public static bool IsObject(this Type type)
    {
        return type == typeof(object);
    }

    public static bool IsValueType(this Type type)
    {
        return type == typeof(ValueType);
    }

    public static bool IsEnum(this Type type)
    {
        return type == typeof(Enum);
    }

    public static bool IsRoot(this Type type)
    {
        return type.IsObject() || type.IsValueType() || type.IsEnum();
    }

    public static bool IsString(this Type type)
    {
        return type == typeof(string);
    }

    public static bool IsNumber(this Type type)
    {
        switch (Type.GetTypeCode(type))
        {
            default:
                return false;

            case TypeCode.Byte:
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.SByte:
            case TypeCode.Single:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
                return true;
        }
    }

    public static bool IsNullableValueType(this Type type)
    {
        return type.IsGenericType && Nullable.GetUnderlyingType(type) != null;
    }

    public static Type? GetNullableValueType(this Type type)
    {
        Type baseType = null;
        if (type.IsGenericType)
        {
            baseType = Nullable.GetUnderlyingType(type);
        }
        return baseType;
    }

    public static Type? BaseClass(this Type type)
    {
        Type baseType = type.BaseType;
        return baseType.IsRoot() ? null : baseType;
    }

    public static IEnumerable<Type> Subclasses(this Type type)
    {
        if (!type.IsClass)
        {
            return Empty<Type>();
        }
        Assembly assembly = type.Assembly;
        return assembly.GetTypes().Where(t => t.IsSubclassOf(type));
    }

    public static IEnumerable<Type> Implementors(this Type type)
    {
        if (!type.IsInterface)
        {
            return Empty<Type>();
        }

        Assembly assembly = type.Assembly;
        return assembly.GetTypes().Where(t => t.IsClass && type.IsAssignableFrom(t));
    }

    public static IEnumerable<CommentAttribute> CommentAttributes(this MemberInfo member)
    {
        object[] attributes = member.GetCustomAttributes(typeof(CommentAttribute), false);
        foreach (var attribute in attributes)
        {
            if (attribute is CommentAttribute comment)
            {
                yield return comment;
            }
        }
    }

    public static IEnumerable<string> Comments(this MemberInfo member)
    {
        return from comment in member.CommentAttributes()
               where comment.HasText
               select comment.Text;
    }

    public static string PropertyName(this MemberInfo member)
    {
        JsonPropertyNameAttribute? attr = (JsonPropertyNameAttribute)member.GetCustomAttribute(typeof(JsonPropertyNameAttribute), true);
        return attr != null ?
                attr.Name :
                member.Name;
    }

    public static VocabAttribute? Vocab(this MemberInfo member)
    {
        return member.GetCustomAttribute(typeof(VocabAttribute)) as VocabAttribute;
    }

    public static string? VocabName(this MemberInfo member)
    {
        VocabAttribute? attr = member.Vocab();
        return attr != null ?
               attr.Name :
               null;
    }

    public static IEnumerable<T> Empty<T>()
    {
        yield break;
    }

}
