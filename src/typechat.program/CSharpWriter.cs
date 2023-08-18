// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

internal class CSharpLang : CodeLanguage
{
    public static class Keywords
    {
        public const string Using = "using";
        public const string Namespace = "namespace";
        public const string Class = "class";
        public const string Return = "return";
    }

    public new class Punctuation : CodeLanguage.Punctuation
    {
        public const string Array = "[]";
    }

    public static class Operators
    {
        public const string Assign = "=";
    }

    public static class Types
    {
        public const string Void = "void";
        public const string Dynamic = "dynamic";
        public const string Var = "var";
        public const string True = "true";
        public const string False = "false";
    }

    public static class Modifiers
    {
        public const string Public = "public";
    }
}

internal class CSharpWriter : CodeWriter
{
    public CSharpWriter(TextWriter writer)
        : base(writer)
    {
    }

    public CSharpWriter Using(string name)
    {
        SOL().Append(CSharpLang.Keywords.Using).Space().Append(name).Semicolon().EOL();
        return this;
    }

    public CSharpWriter Using(IEnumerable<string> names)
    {
        ArgumentNullException.ThrowIfNull(names, nameof(names));
        foreach (string name in names)
        {
            Using(name);
        }
        return this;
    }

    public CSharpWriter Namespace(string name)
    {
        SOL().Append(CSharpLang.Keywords.Namespace).Space().Append(name).Semicolon().EOL();
        return this;
    }

    public CSharpWriter BeginClass(string name, string? baseName = null)
    {
        EOL();
        SOL();
        Public().Append(CSharpLang.Keywords.Class).Space().Append(name);
        if (!string.IsNullOrEmpty(baseName))
        {
            Space().Colon().Space().Append(baseName);
        }
        EOL();
        SOL().LBrace().EOL();
        PushIndent();
        return this;
    }

    public CSharpWriter EndClass()
    {
        PopIndent();
        SOL().RBrace().EOL();
        return this;
    }

    public CSharpWriter BeginDeclareMethod(string name, string? retType)
    {
        SOL();
        if (string.IsNullOrEmpty(retType))
        {
            retType = CSharpLang.Types.Void;
        }
        Public().Append(retType).Space().Append(name).LParan();
        return this;
    }

    public CSharpWriter EndDeclareMethod()
    {
        RParan().EOL();
        return this;
    }

    public CSharpWriter BeginMethodBody()
    {
        SOL().LBrace().EOL();
        PushIndent();
        return this;
    }

    public CSharpWriter EndMethodBody()
    {
        PopIndent();
        SOL().RBrace().EOL();
        return this;
    }

    public CSharpWriter Variable(string name, Type type, int number = 0)
    {
        if (type.IsArray)
        {
            return Variable(name, type.GetElementType().Name, true, number);

        }
        return Variable(name, type.Name, false, number);
    }

    public CSharpWriter Variable(string name, string type, bool isArray, int number = 0)
    {
        if (number > 0)
        {
            Comma().Space();
        }
        Append(type);
        if (isArray)
        {
            Append(CSharpLang.Punctuation.Array);
        }
        Space().Append(name);
        return this;
    }

    public CSharpWriter Local(string name, string type, bool isArray, bool assign = false)
    {
        Variable(name, type, isArray, 0);
        if (assign)
        {
            Space().Append(CSharpLang.Operators.Assign).Space();
        }
        else
        {
            Semicolon().EOL();
        }
        return this;
    }

    public CSharpWriter Return(string? varName = null)
    {
        SOL().Append(CSharpLang.Keywords.Return);
        if (!string.IsNullOrEmpty(varName))
        {
            Space().Append(varName);
        }
        Semicolon().EOL();
        return this;
    }

    public CSharpWriter BeginCall(string variable, string methodName)
    {
        Append(variable).Period().Append(methodName).LParan();
        return this;
    }

    public CSharpWriter EndCall(bool inline = false)
    {
        RParan();
        if (!inline)
        {
            Semicolon().EOL();
        }
        return this;
    }

    public CSharpWriter ArgSep()
    {
        Append(", ");
        return this;
    }

    public CSharpWriter True() { Append(CSharpLang.Types.True); return this; }
    public CSharpWriter False() { Append(CSharpLang.Types.False); return this; }
    public CSharpWriter Literal(string value)
    {
        DoubleQuote().Append(value).DoubleQuote();
        return this;
    }
    public CSharpWriter Literal(double value)
    {
        base.Write(value);
        return this;
    }

    CSharpWriter Public() { Append(CSharpLang.Modifiers.Public).Space(); return this; }
}
