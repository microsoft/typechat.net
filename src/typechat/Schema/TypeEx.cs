// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

internal interface IStringType
{
}
public static class TypeEx
{
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

    public static JsonVocabAttribute? JsonVocabAttribute(this MemberInfo member)
    {
        return member.GetCustomAttribute(typeof(JsonVocabAttribute)) as JsonVocabAttribute;
    }

    public static bool IsString(this Type type)
    {
#if NET7_0_OR_GREATER
        return type == typeof(string) || typeof(IStringType).IsAssignableTo(type);
#else
        return type == typeof(string) || type.IsAssignableFrom(typeof(IStringType));
#endif
    }

    public static bool IsBoolean(this Type type)
    {
        return type == typeof(bool);
    }

    public static bool IsDateTime(this Type type)
    {
        return type == typeof(DateTime) || type == typeof(TimeSpan);
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

    internal static bool IsObject(this Type type)
    {
        return type == typeof(object);
    }

    internal static bool IsValueType(this Type type)
    {
        return type == typeof(ValueType);
    }

    internal static bool IsEnum(this Type type)
    {
        return type == typeof(Enum);
    }

    internal static bool IsRoot(this Type type)
    {
        return type.IsObject() || type.IsValueType() || type.IsEnum();
    }

    internal static bool IsVoid(this Type type)
    {
        return type == typeof(void);
    }

    internal static bool IsAbstract(this PropertyInfo property)
    {
        var methods = property.GetAccessors();
        for (int i = 0; i < methods.Length; ++i)
        {
            if (methods[i].IsAbstract)
            {
                return true;
            }
        }
        return false;
    }

    internal static bool IsRequired(this MemberInfo property)
    {
#if NET7_0_OR_GREATER
        var json_attrib = property.GetCustomAttribute(typeof(JsonRequiredAttribute));
        if (json_attrib != null)
        {
            return true;
        }
#endif
        var attrib = property.GetCustomAttribute(typeof(RequiredAttribute));
        return attrib != null;
    }

    internal static bool IsNullableValueType(this Type type)
    {
        return GetNullableValueType(type) is not null;
    }

    internal static Type? GetNullableValueType(this Type type)
    {
        var foo = Nullable.GetUnderlyingType(type);
        var bar = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        return foo;
    }

    internal static Type? BaseClass(this Type type)
    {
        Type baseType = type.BaseType;
        return baseType.IsRoot() ? null : baseType;
    }

    internal static IEnumerable<Type> Subclasses(this Type type)
    {
        if (!type.IsClass)
        {
            return Empty<Type>();
        }
        Assembly assembly = type.Assembly;
        return assembly.GetTypes().Where(t => t.IsSubclassOf(type));
    }

    internal static IEnumerable<Type> Implementors(this Type type)
    {
        if (!type.IsInterface)
        {
            return Empty<Type>();
        }

        Assembly assembly = type.Assembly;
#if NET7_0_OR_GREATER
        return assembly.GetTypes().Where(t => t.IsClass && type.IsAssignableTo(t));
#else
        return assembly.GetTypes().Where(t => t.IsClass && t.IsAssignableFrom(type));
#endif
    }


    internal static bool IsIgnore(this MemberInfo member)
    {
        JsonIgnoreAttribute? attr = (JsonIgnoreAttribute)member.GetCustomAttribute(typeof(JsonIgnoreAttribute), true);
        return attr != null;
    }

    internal static string PropertyName(this MemberInfo member)
    {
        JsonPropertyNameAttribute? attr = (JsonPropertyNameAttribute)member.GetCustomAttribute(typeof(JsonPropertyNameAttribute), true);
        return attr != null ?
                attr.Name :
                member.Name;
    }

    internal static IEnumerable<T> Empty<T>()
    {
        yield break;
    }

}
