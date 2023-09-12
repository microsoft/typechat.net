// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;

namespace Microsoft.TypeChat.Schema;

internal static class NullableTypeExtensions
{
    const string NullableAttributeFullName = "System.Runtime.CompilerServices.NullableAttribute";
    const string NullableContextAttributeFullName = "System.Runtime.CompilerServices.NullableContextAttribute";

    private enum NullableState
    {
        // From https://github.com/dotnet/roslyn/blob/main/docs/features/nullable-metadata.md
        Oblivious = 0,
        NotNullable = 1,
        Nullable = 2
    }

#if NET6_0_OR_GREATER
    private static NullabilityInfoContext s_nullableInfo = new();
#endif

    public static bool IsNullable(this MemberInfo member)
    {
        if (member.DeclaringType.IsValueType)
        {
            return Nullable.GetUnderlyingType(member.DeclaringType) != null;
        }

        switch (member)
        {
            case PropertyInfo propertyInfo:
                return propertyInfo.IsNullable();

            case FieldInfo fieldInfo:
                return fieldInfo.IsNullable();
        }

        return false;
    }

    public static bool IsNullable(this PropertyInfo propInfo)
    {
#if NET6_0_OR_GREATER
        return s_nullableInfo.Create(propInfo).WriteState is NullabilityState.Nullable;
#else
        return IsNullableLegacyImpl(propInfo.PropertyType, propInfo.DeclaringType, propInfo.CustomAttributes);
#endif
    }

    public static bool IsNullable(this FieldInfo fieldInfo)
    {
#if NET6_0_OR_GREATER
        return s_nullableInfo.Create(fieldInfo).WriteState is NullabilityState.Nullable;
#else
        return IsNullableLegacyImpl(fieldInfo.FieldType, fieldInfo.DeclaringType, fieldInfo.CustomAttributes);
#endif
    }

    public static bool IsNullable(this ParameterInfo paramInfo)
    {
#if NET6_0_OR_GREATER
        return s_nullableInfo.Create(paramInfo).WriteState is NullabilityState.Nullable;
#else
        return IsNullableLegacyImpl(paramInfo.ParameterType, paramInfo.Member, paramInfo.CustomAttributes);
#endif
    }

    private static bool IsNullableLegacyImpl(
        Type type,
        MemberInfo? declaringType,
        IEnumerable<CustomAttributeData> customAttributes)
    {
        if (type.IsValueType)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        var nullableAttribute = customAttributes
            .FirstOrDefault(x => x.AttributeType.FullName == NullableAttributeFullName);
        if (nullableAttribute?.ConstructorArguments.Count == 1)
        {
            var attributeArg = nullableAttribute.ConstructorArguments[0];
            switch (attributeArg.ArgumentType)
            {
                case Type byteType when byteType == typeof(byte):
                    return IsNullableConstructorArg(attributeArg);

                case Type byteArrayType when byteArrayType == typeof(byte[]):
                    var args = (ReadOnlyCollection<CustomAttributeTypedArgument>)attributeArg.Value;
                    return IsNullableConstructorArg(args.FirstOrDefault());
            }
        }

        for (var decType = declaringType; decType is not null; decType = decType.DeclaringType)
        {
            var contextAttribute = decType.CustomAttributes
                .FirstOrDefault(x => x.AttributeType.FullName == NullableContextAttributeFullName);
            if (contextAttribute is not null)
            {
                return IsNullableConstructorArg(contextAttribute.ConstructorArguments.FirstOrDefault());
            }
        }

        return false;
    }

    private static bool IsNullableConstructorArg(CustomAttributeTypedArgument arg)
    {
        switch (arg.ArgumentType)
        {
            case Type byteType when byteType == typeof(byte):
                return (byte)arg.Value == (byte)NullableState.Nullable;

            case Type byteArrayType when byteArrayType == typeof(byte[]):
                var args = (ReadOnlyCollection<CustomAttributeTypedArgument>)arg.Value;
                return IsNullableConstructorArg(args.FirstOrDefault());
        }

        return false;
    }
}
