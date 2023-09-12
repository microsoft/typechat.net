// Copyright (c) Microsoft. All rights reserved.

using Microsoft.TypeChat.Schema;

namespace Microsoft.TypeChat;

internal class CSharpLang : CodeLanguage
{
    public static class Keywords
    {
        public const string Using = "using";
        public const string Namespace = "namespace";
        public const string Class = "class";
        public const string Return = "return";
        public const string New = "new";
        public const string Async = "async";
        public const string Await = "await";
    }

    public static string TypeOf(string type)
    {
        return $"typeof({type})";
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
        EOL().SOL();
        Public().Append(CSharpLang.Keywords.Class).Space().Append(name);
        if (!string.IsNullOrEmpty(baseName))
        {
            Space().Colon().Space().Append(baseName);
        }
        EOL().SOL().LBrace().EOL();
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

    public CSharpWriter DeclareMethod(string name, string? retType)
    {
        return BeginDeclareMethod(name, retType).EndDeclareMethod();
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
            ArgSep();
        }
        Append(type);
        if (isArray)
        {
            LSquare().RSquare();
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

    public CSharpWriter BeginMethodCall(string objName, string methodName)
    {
        Append(objName).Period().Append(methodName).LParan();
        return this;
    }

    public CSharpWriter Args(params string[] values)
    {
        if (values != null)
        {
            for (int i = 0; i < values.Length; ++i)
            {
                if (i > 0)
                {
                    ArgSep();
                }
                Append(values[i]);
            }
        }
        return this;
    }

    public CSharpWriter EndMethodCall(bool inline = false)
    {
        RParan();
        if (!inline)
        {
            Semicolon().EOL();
        }
        return this;
    }

    public CSharpWriter MethodCall(string objName, string methodName, params string[] args)
    {
        BeginMethodCall(objName, methodName);
        {
            Args(args);
        }
        EndMethodCall();
        return this;
    }

    public CSharpWriter StaticCall(string methodName, params string[] args)
    {
        Append(methodName).LParan();
        {
            Args(args);
        }
        RParan();

        return this;
    }

    public CSharpWriter Cast(string type)
    {
        LParan().Append(type).RParan();
        return this;
    }

    public CSharpWriter New(string type)
    {
        Append(CSharpLang.Keywords.New).Space();
        StaticCall(type);
        return this;
    }

    public CSharpWriter Array()
    {
        LSquare().RSquare();
        return this;
    }

    public CSharpWriter BeginArray(string type, int length = 0)
    {
        Append(CSharpLang.Keywords.New).Space();
        Append(type);
        if (length > 0)
        {
            LSquare().Append(length).RBrace();
        }
        else
        {
            Array().LBrace();
        }
        return this;
    }

    public CSharpWriter EndArray()
    {
        RBrace();
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
