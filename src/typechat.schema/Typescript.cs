// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

public class Typescript : CodeLanguage
{
    public new class Punctuation : CodeLanguage.Punctuation
    {
        public const string Array = "[]";
    }

    public static class Operators
    {
        public const string Or = "|";
        public const string Assign = "=";
    }

    public static class Keywords
    {
        public const string Export = "export";
        public const string Interface = "interface";
        public const string Type = "type";
        public const string Extends = "extends";
        public const string Enum = "enum";
    }

    public static class Types
    {
        public const string String = "string";
        public const string Number = "number";
        public const string Void = "void";
        public const string Any = "any";

        public static string? ToPrimitive(Type type)
        {
            if (type.IsString())
            {
                return Types.String;
            }
            else if (type.IsEnum)
            {
                return null;
            }
            else if (type.IsNumber())
            {
                return Types.Number;
            }
            else if (type.IsObject())
            {
                return Types.Any;
            }
            else if (type.IsVoid())
            {
                return Types.Void;
            }
            return null;
        }
    }
}
