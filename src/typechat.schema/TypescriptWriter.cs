// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

public class TypescriptWriter
{
    CodeWriter _writer;

    public TypescriptWriter(TextWriter writer)
        : this(new CodeWriter(writer))
    {
    }

    public TypescriptWriter(CodeWriter writer)
    {
        _writer = writer;
    }

    public CodeWriter Writer => _writer;

    public TypescriptWriter PushIndent()
    {
        _writer.PushIndent();
        return this;
    }

    public TypescriptWriter PopIndent()
    {
        _writer.PopIndent();
        return this;
    }

    public virtual TypescriptWriter Clear()
    {
        _writer.Clear();
        return this;
    }

    public TypescriptWriter Append(string token)
    {
        if (!string.IsNullOrEmpty(token))
        {
            _writer.Write(token);
        }
        return this;
    }

    public TypescriptWriter Space()
    {
        _writer.Write(Typescript.Punctuation.Space);
        return this;
    }

    // Start a line
    public TypescriptWriter SOL()
    {
        _writer.WriteIndent();
        return this;
    }

    public TypescriptWriter EOL()
    {
        _writer.Write(Typescript.Punctuation.EOL);
        return this;
    }

    public TypescriptWriter LBrace() { _writer.LBrace(); return this; }
    public TypescriptWriter RBrace() { _writer.RBrace(); return this; }
    public TypescriptWriter LParan() { _writer.LParan(); return this; }
    public TypescriptWriter RParan() { _writer.RParan(); return this; }
    public TypescriptWriter Semicolon() { _writer.Semicolon(); return this; }
    public TypescriptWriter Comma() { _writer.Comma(); return this; }
    public TypescriptWriter EOS() => Semicolon().EOL();
    public TypescriptWriter Colon() { _writer.Colon(); return this; }
    public TypescriptWriter Assign() => Append(Typescript.Operators.Assign);
    public TypescriptWriter Or() => Append(Typescript.Operators.Or);
    public TypescriptWriter Comment(string text)
    {
        return Append(Typescript.Punctuation.Comment).Space().Append(text).EOL();
    }

    public TypescriptWriter StartBlock() => LBrace().EOL();
    public TypescriptWriter EndBlock() => RBrace().EOL();

    public TypescriptWriter Export() => Append(Typescript.Keywords.Export);
    public TypescriptWriter Extends() => Append(Typescript.Keywords.Extends);

    public TypescriptWriter Name(string name) => Append(name);
    public TypescriptWriter Name(string name, bool nullable)
    {
        Name(name);
        if (nullable)
        {
            _writer.Question();
        }
        return this;
    }
    public TypescriptWriter DataType(string name) => Colon().Space().Name(name);
    public TypescriptWriter Array() => Append(Typescript.Punctuation.Array);
    public TypescriptWriter Literal(string value)
    {
        _writer.SQuote().Write(value).SQuote();
        return this;
    }

    public TypescriptWriter Literals(IEnumerable<string> values)
    {
        ArgumentNullException.ThrowIfNull(values, nameof(values));
        int i = 0;
        foreach (var value in values)
        {
            if (i > 0)
            {
                Space().Or().Space();
            }
            Literal(value);
            ++i;
        }
        return this;
    }

    public TypescriptWriter Variable(
        string name,
        string dataType,
        bool isArray = false,
        bool nullable = false)
    {
        return DeclareVariable(name, dataType, isArray, nullable).Semicolon();
    }

    public TypescriptWriter Variable(string name, bool nullable, IEnumerable<string> literals)
    {
        Name(name, nullable).Colon().Space().Literals(literals).Semicolon();
        return this;
    }

    public TypescriptWriter DeclareVariable(
        string name,
        string dataType,
        bool isArray = false,
        bool nullable = false)
    {
        Name(name, nullable).Colon().Space().Name(dataType);
        if (isArray)
        {
            Array();
        }
        return this;
    }

    public TypescriptWriter Argument(
        string name,
        string dataType,
        int argNumber,
        int argCount,
        bool isArray = false,
        bool nullable = false)
    {
        if (argNumber > 0)
        {
            Space();
        }
        DeclareVariable(name, dataType, isArray, nullable);
        if (argNumber < argCount - 1)
        {
            Comma();
        }
        return this;
    }

    public TypescriptWriter Type(string name)
    {
        return Append(Typescript.Keywords.Type).Space().Name(name);
    }

    public TypescriptWriter Enum(string name)
    {
        return Append(Typescript.Keywords.Enum).Space().Name(name);
    }

    public TypescriptWriter BeginType(string name)
    {
        return Type(name).Space().StartBlock();
    }

    public TypescriptWriter EndType(string name)
    {
        // TODO: validation here to verify Begin & End match
        return EndBlock();
    }

    public TypescriptWriter Interface(string name, bool space = true)
    {
        return Append(Typescript.Keywords.Interface).Space().Name(name);
    }

    public TypescriptWriter BeginInterface(string name, string baseType = null)
    {
        if (string.IsNullOrEmpty(baseType))
        {
            Interface(name).Space().StartBlock();
        }
        else
        {
            Interface(name).Space().Extends().Space().Name(baseType).Space().StartBlock();
        }
        return this;
    }

    public TypescriptWriter EndInterface(string name)
    {
        // TODO: validation here to verify Begin & End match
        return SOL().EndBlock();
    }

    public TypescriptWriter BeginMethodDeclare(string name)
    {
        return Name(name).LParan();
    }

    public TypescriptWriter EndMethodDeclare(string? returnType = null)
    {
        RParan();
        if (!string.IsNullOrEmpty(returnType))
        {
            Colon().Space().Name(returnType);
        }
        return Semicolon();
    }

    public static string WriteCode(Action<TypescriptWriter> codeGen)
    {
        ArgumentNullException.ThrowIfNull(codeGen, nameof(codeGen));

        using StringWriter sw = new StringWriter();
        TypescriptWriter writer = new TypescriptWriter(sw);
        codeGen(writer);
        sw.Flush();
        return sw.ToString();
    }
}
