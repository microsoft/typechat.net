// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

public static class TypeExtensions
{
    public static bool IsString(this Type type)
    {
        return type == typeof(string) || typeof(IStringType).IsAssignableFrom(type);
    }

    internal static bool IsBoolean(this Type type)
    {
        return type == typeof(bool);
    }

    internal static bool IsDateTime(this Type type)
    {
        return type == typeof(DateTime) || type == typeof(TimeSpan);
    }

    internal static bool IsNumber(this Type type)
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
