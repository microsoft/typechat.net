﻿// Copyright (c) Microsoft. All rights reserved.

using Microsoft.TypeChat.Schema;

namespace Microsoft.TypeChat;

public class ProgramWriter
{
    CodeWriter _writer;

    public ProgramWriter(TextWriter writer)
    {
        _writer = new CodeWriter(writer);
    }

    public string ProgramName { get; set; } = "program";
    public string ApiVarName { get; set; } = "api";
    public string ResultVarPrefix { get; set; } = "step";

    public CodeWriter Writer => _writer;

    public void Clear()
    {
        _writer.Clear();
    }

    public ProgramWriter Call(string functionName, dynamic[] args, bool inline = false)
    {
        _writer.SOL();
        BeginCall(functionName);
        for (int i = 0; i < args.Length; ++i)
        {
            if (i > 0) { _writer.Comma().Space(); }
            _writer.Append(Convert.ToString(args[i]));
        }
        EndCall(inline);
        return this;
    }

    public ProgramWriter Write(Program program, Type apiType)
    {
        ArgumentNullException.ThrowIfNull(program, nameof(program));

        BeginMethodDeclare(ProgramName);
        {
            DeclareVariable(ApiVarName, apiType.Name);
        }
        EndMethodDeclare();
        Write(program.Steps);
        EndMethod();
        return this;
    }

    public ProgramWriter Write(Steps steps)
    {
        FunctionCall[] calls = steps.Calls;
        for (int i = 0; i < calls.Length; ++i)
        {
            _writer.SOL();
            DeclareVariable(ResultVarName(i)).Assign();
            Write(calls[i]);
        }
        if (calls.Length > 0)
        {
            _writer.SOL().Write($"return {ResultVarName(calls.Length - 1)}").EOL();
        }
        return this;
    }

    public ProgramWriter Write(FunctionCall call, bool inline = false)
    {
        ArgumentNullException.ThrowIfNull(call, nameof(call));

        BeginCall(call.Name, ApiVarName);
        if (call.Args != null)
        {
            WriteArgs(call.Args);
        }
        EndCall(inline);
        return this;
    }

    public ProgramWriter WriteArgs(Expression[] args)
    {
        ArgumentNullException.ThrowIfNull(args, nameof(args));

        for (int i = 0; i < args.Length; ++i)
        {
            if (i > 0) { _writer.Comma().Space(); }
            Write(args[i]);
        }
        return this;
    }

    public ProgramWriter Write(ValueExpr expr)
    {
        _writer.Append(expr.Value.ToString());
        return this;
    }

    ProgramWriter Write(Expression expr)
    {
        switch (expr)
        {
            default:
                BeginComment();
                {
                    _writer.Space().Append(Json.Stringify(expr.Source)).Space();
                }
                EndComment();
                break;

            case FunctionCall call:
                Write(call, true);
                break;

            case ResultReference resultRef:
                _writer.Append(ResultVarName(resultRef.Ref));
                break;

            case ValueExpr valueExpr:
                Write(valueExpr);
                break;
        }
        return this;
    }

    string ResultVarName(int resultNumber)
    {
        return (ResultVarPrefix + resultNumber);
    }

    ProgramWriter Assign() { _writer.Space().Append(CSharp.Operators.Assign).Space(); return this; }

    ProgramWriter Comment(string text)
    {
        _writer.Append(CodeLanguage.Punctuation.Comment).Space().Append(text).EOL();
        return this;
    }

    ProgramWriter BeginComment()
    {
        _writer.Append(CodeLanguage.Punctuation.LComment);
        return this;
    }

    ProgramWriter EndComment()
    {
        _writer.Append(CodeLanguage.Punctuation.RComment);
        return this;
    }

    ProgramWriter BeginMethodDeclare(string name, string? returnType = null)
    {
        returnType ??= CSharp.Types.Dynamic;
        _writer.SOL().Append(returnType).Space();
        _writer.Append(name).LParan();
        return this;
    }

    ProgramWriter DeclareVariable(string name, bool isArray = false, bool nullable = false)
    {
        return DeclareVariable(name, CSharp.Types.Var, isArray, nullable);
    }

    ProgramWriter DeclareVariable(string name, string dataType, bool isArray = false, bool nullable = false)
    {
        _writer.Append(dataType);
        if (isArray)
        {
            _writer.Append(CSharp.Punctuation.Array);
        }
        if (nullable)
        {
            _writer.Question();
        }
        _writer.Space().Append(name);
        return this;
    }

    ProgramWriter EndMethodDeclare()
    {
        _writer.RParan().EOL().LBrace().EOL();
        _writer.PushIndent();
        return this;
    }

    ProgramWriter EndMethod()
    {
        _writer.RBrace().EOL();
        _writer.PopIndent();
        return this;
    }

    ProgramWriter BeginCall(string name, string instanceVar = null)
    {
        if (instanceVar != null)
        {
            _writer.Append(instanceVar).Period();
        }
        _writer.Append(name).LParan();
        return this;
    }

    ProgramWriter EndCall(bool inline = false)
    {
        _writer.RParan();
        if (!inline)
        {
            _writer.Semicolon().EOL();
        }
        return this;
    }

    class CSharp : CodeLanguage
    {
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
        }
    }
}
