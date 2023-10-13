// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

/// <summary>
/// Class with methods to support writing Typescript code
/// This class is reserved for TypeChat infrastructure
/// </summary>
public class TypescriptWriter
{
    /// <summary>
    /// Create a new TypescriptWrite that writes Typescript to the given text writer
    /// </summary>
    /// <param name="writer"></param>
    public TypescriptWriter(TextWriter writer)
    {
        Writer = new CodeWriter(writer);
    }

    /// <summary>
    /// The underlying Code Writer
    /// </summary>
    public CodeWriter Writer { get; }

    /// <summary>
    /// Increase indent
    /// </summary>
    /// <returns></returns>
    public TypescriptWriter PushIndent()
    {
        Writer.PushIndent();
        return this;
    }

    /// <summary>
    /// Decrease indent
    /// </summary>
    /// <returns></returns>
    public TypescriptWriter PopIndent()
    {
        Writer.PopIndent();
        return this;
    }

    /// <summary>
    /// Reset the writer's state. 
    /// </summary>
    /// <returns></returns>
    public virtual TypescriptWriter Clear()
    {
        Writer.Clear();
        return this;
    }

    /// <summary>
    /// Flush current output
    /// </summary>
    public void Flush() => Writer.Flush();

    public TypescriptWriter Append(string token) { Writer.Append(token); return this; }

    public TypescriptWriter Space() { Writer.Space(); return this; }

    public TypescriptWriter SOL() { Writer.SOL(); return this; } // Start a line

    public TypescriptWriter EOL() { Writer.EOL(); return this; } // End of line

    public TypescriptWriter LBrace() { Writer.LBrace(); return this; }

    public TypescriptWriter RBrace() { Writer.RBrace(); return this; }

    public TypescriptWriter LParan() { Writer.LParan(); return this; }

    public TypescriptWriter RParan() { Writer.RParan(); return this; }

    public TypescriptWriter Semicolon() { Writer.Semicolon(); return this; }

    public TypescriptWriter Comma() { Writer.Comma(); return this; }

    public TypescriptWriter EOS() => Semicolon().EOL();

    public TypescriptWriter Colon() { Writer.Colon(); return this; }

    public TypescriptWriter Assign() => Append(Typescript.Operators.Assign);

    public TypescriptWriter Or() => Append(Typescript.Operators.Or);

    public TypescriptWriter Comment(string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            Append(CodeLanguage.Punctuation.Comment).Space().Append(text).EOL();
        }

        return this;
    }

    /// <summary>
    /// Start a code block
    /// </summary>
    /// <returns></returns>
    public TypescriptWriter StartBlock() => LBrace().EOL();

    /// <summary>
    /// End a code block
    /// </summary>
    /// <returns></returns>
    public TypescriptWriter EndBlock() => RBrace().EOL();

    public TypescriptWriter Export() => Append(Typescript.Keywords.Export);

    public TypescriptWriter Extends() => Append(Typescript.Keywords.Extends);

    public TypescriptWriter Name(string name, bool isNullable = false)
    {
        Append(name);
        if (isNullable)
        {
            Writer.Question();
        }

        return this;
    }

    public TypescriptWriter DataType(string name) => Colon().Space().Name(name);

    public TypescriptWriter Array() => Append(Typescript.Punctuation.Array);

    public TypescriptWriter Literal(string value)
    {
        Writer.SQuote().Append(value).SQuote();
        return this;
    }

    public TypescriptWriter Literals(IEnumerable<string> values)
    {
        ArgumentVerify.ThrowIfNull(values, nameof(values));
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
        bool isNullable = false)
    {
        return DeclareVariable(name, dataType, isArray, isNullable).Semicolon();
    }

    public TypescriptWriter Variable(string name, bool isNullable, IEnumerable<string> literals)
    {
        Name(name, isNullable).Colon().Space().Literals(literals).Semicolon();
        return this;
    }

    public TypescriptWriter DeclareVariable(
        string name,
        string dataType,
        bool isArray = false,
        bool isNullable = false)
    {
        Name(name, isNullable).Colon().Space().Name(dataType);
        if (isArray)
        {
            Array();
        }

        return this;
    }

    public TypescriptWriter Parameter(
        string name,
        string dataType,
        int argNumber,
        int argCount,
        bool isArray = false,
        bool isNullable = false)
    {
        if (argNumber > 0)
        {
            Space();
        }

        DeclareVariable(name, dataType, isArray, isNullable);
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

    public TypescriptWriter Interface(string name)
    {
        return Append(Typescript.Keywords.Interface).Space().Name(name);
    }

    public TypescriptWriter BeginInterface(string name, string baseType = null)
    {
        Interface(name).Space();
        if (!string.IsNullOrEmpty(baseType))
        {
            Extends().Space().Name(baseType).Space();
        }

        return StartBlock();
    }

    public TypescriptWriter EndInterface()
    {
        return SOL().EndBlock();
    }

    public TypescriptWriter BeginMethodDeclare(string name)
    {
        return SOL().Name(name).LParan();
    }

    public TypescriptWriter EndMethodDeclare(string? returnType = null, bool nullable = false)
    {
        RParan();
        if (!string.IsNullOrEmpty(returnType))
        {
            Colon().Space().Name(returnType);
            if (nullable)
            {
                Writer.Question();
            }
        }

        return EOS();
    }

    /// <summary>
    /// Wrapper method that sets up a TypescriptWriter, then calls the given codeGenerator
    /// When codeGeneration completes, returns the generated code as a string
    /// </summary>
    /// <param name="codeGenerator">Callback that generates code</param>
    /// <returns></returns>
    public static string WriteCode(Action<TypescriptWriter> codeGenerator)
    {
        ArgumentVerify.ThrowIfNull(codeGenerator, nameof(codeGenerator));

        using StringWriter sw = new StringWriter();
        TypescriptWriter writer = new TypescriptWriter(sw);
        codeGenerator(writer);
        sw.Flush();
        return sw.ToString();
    }
}
