// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

/// <summary>
/// Terminals, Operators etc, used to export Typescript
/// This only contains enough operators currently used for schema export
/// </summary>
public class Typescript : CodeLanguage
{
    public new class Punctuation : CodeLanguage.Punctuation
    {
        public const string Array = "[]";
    }

    /// <summary>
    /// Typescript operators. Not exhaustive.
    /// </summary>
    public static class Operators
    {
        public const string Or = "|";
        public const string Assign = "=";
    }

    /// <summary>
    /// Typescript keywords used during export. Not exhaustive
    /// </summary>
    public static class Keywords
    {
        public const string Export = "export";
        public const string Interface = "interface";
        public const string Type = "type";
        public const string Extends = "extends";
        public const string Enum = "enum";
    }

    /// <summary>
    /// Typescript primitive types
    /// </summary>
    public static class Types
    {
        public const string String = "string";
        public const string Number = "number";
        public const string Boolean = "boolean";
        public const string Void = "void";
        public const string Any = "any";

        /// <summary>
        /// Convert the given .NET type to its Typescript primitive, if any
        /// </summary>
        /// <param name="type">.NET type to map</param>
        /// <returns>Typescript type or null</returns>
        public static string? ToPrimitive(Type type)
        {
            if (type.IsString())
            {
                return String;
            }

            if (type.IsEnum)
            {
                return null;
            }

            if (type.IsNumber())
            {
                return Number;
            }

            if (type.IsBoolean())
            {
                return Boolean;
            }

            if (type.IsDateTime())
            {
                return String; // Json does not have a primitive DateTime
            }

            if (type.IsObject())
            {
                return Any;
            }

            if (type.IsVoid())
            {
                return Void;
            }

            return null;
        }
    }
}
