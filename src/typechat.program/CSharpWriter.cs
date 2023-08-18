// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

internal class CSharpLang
{
    public static class Keywords
    {
        public const string Using = "using";
        public const string Namespace = "namespace";
        public const string Class = "class";
        public const string Return = "return";
    }

    public static class Types
    {
        public const string Void = "void";
        public const string Dynamic = "dynamic";
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
        return this;
    }

    public CSharpWriter EndClass()
    {
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

    public CSharpWriter Parameter(string name, Type type, int number = 0)
    {
        return Parameter(name, type.Name, number);
    }

    public CSharpWriter Parameter(string name, string type, int number = 0)
    {
        if (number > 0)
        {
            Comma().Space();
        }
        Append(type).Space().Append(name);
        return this;
    }

    public CSharpWriter EndDeclareMethod()
    {
        RParan().EOL();
        return this;
    }

    public CSharpWriter BeginMethodBody()
    {
        SOL().LBrace().EOL().SOL();
        return this;
    }

    public CSharpWriter EndMethodBody()
    {
        SOL().RBrace().EOL();
        return this;
    }

    public CSharpWriter Return()
    {
        SOL().Append(CSharpLang.Keywords.Return).Semicolon().EOL();
        return this;
    }

    CSharpWriter Public() { Append(CSharpLang.Modifiers.Public).Space(); return this; }
}
