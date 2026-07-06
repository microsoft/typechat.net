// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

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
        return type == typeof(string) || typeof(IStringType).IsAssignableFrom(type);
    }

    public static bool IsBoolean(this Type type)
    {
        return type == typeof(bool);
    }

    public static bool IsDateTime(this Type type)
    {
        // These are value types that System.Text.Json serializes to/from a JSON *string* rather than
        // to an object. They must be treated as the 'string' primitive by the exporter (see
        // Typescript.Types.ToPrimitive); otherwise they would be exported as (mostly empty) interfaces.
        if (type == typeof(DateTime) || type == typeof(TimeSpan) || type == typeof(DateTimeOffset))
        {
            return true;
        }
#if NET6_0_OR_GREATER
        // DateOnly/TimeOnly only exist on net6.0+, but Json serializes them as strings too.
        if (type == typeof(DateOnly) || type == typeof(TimeOnly))
        {
            return true;
        }
#endif
        return false;
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
        if (json_attrib is not null)
        {
            return true;
        }
#endif
        var attrib = property.GetCustomAttribute(typeof(RequiredAttribute));
        return attrib is not null;
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

    internal static bool IsTask(this Type type)
    {
        return typeof(Task).IsAssignableFrom(type);
    }

    internal static Type? GetGenericType(this Type type)
    {
        if (!type.IsGenericType)
        {
            return null;
        }
        var args = type.GetGenericArguments();
        return (args.IsNullOrEmpty()) ? null : args[0];
    }

    /// <summary>
    /// Is the given type one that serializes to a JSON array? This includes arrays and any type
    /// that implements IEnumerable{T} (List{T}, IList{T}, ICollection{T}, HashSet{T}...).
    /// Strings (which are IEnumerable{char}) and dictionaries are deliberately excluded: they do
    /// not serialize to JSON arrays.
    /// </summary>
    /// <param name="type">type to inspect</param>
    /// <param name="elementType">the type of the items in the array</param>
    /// <returns>true if the type is array-like</returns>
    internal static bool IsArrayLike(this Type type, out Type elementType)
    {
        elementType = null;

        // Strings are IEnumerable<char> but must be exported as scalar strings, not char arrays
        if (type.IsString())
        {
            return false;
        }
        // Dictionaries are IEnumerable<KeyValuePair<,>> but must be exported as maps, not arrays.
        // Use a structural check so that even a self-mapping dictionary is excluded here.
        if (type.IsDictionaryShaped())
        {
            return false;
        }
        if (type.IsArray)
        {
            elementType = type.GetElementType();
            return true;
        }

        Type? enumerableType = FindGenericInterface(type, typeof(IEnumerable<>));
        if (enumerableType is not null)
        {
            Type genericArg = enumerableType.GetGenericArguments()[0];
            // Guard against a type that enumerates itself (e.g. class Node : IEnumerable<Node>):
            // there is no finite element type to unwrap to, so treat it as a plain object instead.
            if (genericArg != type)
            {
                elementType = genericArg;
                return true;
            }
            return false;
        }
        // Non-generic IEnumerable (e.g. ArrayList): the element type is unknown, so treat it as 'any'
        if (typeof(IEnumerable).IsAssignableFrom(type))
        {
            elementType = typeof(object);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Is the given type one that serializes to a JSON object/map? This includes any type that
    /// implements IDictionary{K,V} or IReadOnlyDictionary{K,V}, as well as the non-generic IDictionary.
    /// </summary>
    /// <param name="type">type to inspect</param>
    /// <param name="keyType">the type of the map's keys</param>
    /// <param name="valueType">the type of the map's values</param>
    /// <returns>true if the type is a dictionary</returns>
    internal static bool IsDictionary(this Type type, out Type keyType, out Type valueType)
    {
        keyType = typeof(string);
        valueType = typeof(object);

        Type? dictionaryType = FindGenericInterface(type, typeof(IDictionary<,>)) ??
                               FindGenericInterface(type, typeof(IReadOnlyDictionary<,>));
        if (dictionaryType is not null)
        {
            Type[] args = dictionaryType.GetGenericArguments();
            // Guard against a type that maps to itself (e.g. class D : IDictionary<string, D>):
            // there is no finite value type to unwrap to, so treat it as a plain object instead.
            if (args[1] != type)
            {
                keyType = args[0];
                valueType = args[1];
                return true;
            }
            return false;
        }
        // Non-generic IDictionary (e.g. Hashtable): keys and values are untyped
        return typeof(IDictionary).IsAssignableFrom(type);
    }

    private static Type? FindGenericInterface(Type type, Type genericInterface)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == genericInterface)
        {
            return type;
        }
        foreach (Type iface in type.GetInterfaces())
        {
            if (iface.IsGenericType && iface.GetGenericTypeDefinition() == genericInterface)
            {
                return iface;
            }
        }
        return null;
    }

    private static bool IsDictionaryShaped(this Type type)
    {
        return FindGenericInterface(type, typeof(IDictionary<,>)) is not null ||
               FindGenericInterface(type, typeof(IReadOnlyDictionary<,>)) is not null ||
               typeof(IDictionary).IsAssignableFrom(type);
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

        return assembly.GetTypes().Where(t => t.IsClass && t.IsAssignableFrom(type));
    }


    internal static bool IsIgnore(this MemberInfo member)
    {
        JsonIgnoreAttribute? attr = (JsonIgnoreAttribute)member.GetCustomAttribute(typeof(JsonIgnoreAttribute), true);
        return attr is not null;
    }

    internal static string PropertyName(this MemberInfo member)
    {
        JsonPropertyNameAttribute? attr = (JsonPropertyNameAttribute)member.GetCustomAttribute(typeof(JsonPropertyNameAttribute), true);
        return attr is not null ?
                attr.Name :
                member.Name;
    }

    internal static string GenerateInterfaceTypeDef(this Type type)
    {
        if (!type.IsGenericType)
        {
            return type.Name;
        }

        string typeName = type.NonGenericName();
        Type typeDef = type.GetGenericTypeDefinition();
        Type[] typeParams = typeDef.GetGenericArguments();
        if (typeParams.IsNullOrEmpty())
        {
            return typeName;
        }
        StringBuilder sb = new StringBuilder();
        sb.Append(typeName).Append('<');
        typeParams.CombineArgNames(sb, ",").Append('>');
        return sb.ToString();
    }

    internal static string GenerateInterfaceName(this Type type)
    {
        if (!type.IsGenericType)
        {
            return type.Name;
        }

        string typeName = type.NonGenericName();
        Type[] typeArgs = type.GetGenericArguments();
        if (typeArgs.IsNullOrEmpty())
        {
            return typeName;
        }
        StringBuilder sb = new StringBuilder();
        sb.Append(typeName).Append('_');
        typeArgs.CombineArgNames(sb, "_");
        return sb.ToString();
    }

    internal static StringBuilder CombineArgNames(this Type[] args, StringBuilder sb, string sep)
    {
        var paramNames = from arg in args select arg.Name;
        sb.AppendMultiple(sep, paramNames);
        return sb;
    }

    internal static string NonGenericName(this Type type)
    {
        string name = type.Name;
        int i = name.IndexOf('`');
        if (i >= 0)
        {
            return name.Substring(0, i);
        }
        return name;
    }

    internal static IEnumerable<T> Empty<T>()
    {
        yield break;
    }
}
