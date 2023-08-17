// Copyright (c) Microsoft. All rights reserved.

using Microsoft.TypeChat.Schema;

namespace Microsoft.TypeChat;

/// <summary>
/// Simple Program writer: writes out Program as a C# function
/// </summary>
public class ProgramWriter
{
    CodeWriter _writer;

    public ProgramWriter(TextWriter writer)
    {
        _writer = new CodeWriter(writer);
    }

    public string ProgramName { get; set; } = "Program";
    public string ApiVarName { get; set; } = "api";
    public string ResultVarPrefix { get; set; } = "step";

    public CodeWriter Writer => _writer;

    public void Clear()
    {
        _writer.Clear();
    }

    /// <summary>
    /// Write the given function call...
    /// </summary>
    public ProgramWriter Write(string functionName, dynamic[] args, bool isCallInline = false)
    {
        _writer.SOL();
        BeginCall(functionName);
        for (int i = 0; i < args.Length; ++i)
        {
            if (i > 0) { _writer.Append($", "); }
            _writer.Append(Convert.ToString(args[i]));
        }
        EndCall(isCallInline);
        return this;
    }

    public ProgramWriter Write(Program program, Type apiType)
    {
        ArgumentNullException.ThrowIfNull(program, nameof(program));

        SOL().Write($"dynamic {ProgramName}({apiType.Name} {ApiVarName}) {{").EOL();

        _writer.PushIndent();
        Write(program.Steps).Write("}").EOL();
        _writer.PopIndent();

        return this;
    }

    ProgramWriter Write(Steps steps)
    {
        FunctionCall[] calls = steps.Calls;
        if (calls != null && calls.Length > 0)
        {
            for (int i = 0; i < calls.Length; ++i)
            {
                _writer.SOL().Append($"var {ResultVar(i)} = ");
                Write(calls[i]);
            }
            _writer.SOL().Write($"return {ResultVar(calls.Length - 1)};").EOL();
        }
        return this;
    }

    ProgramWriter Write(FunctionCall call, bool inline = false)
    {
        return BeginCall(call.Name, ApiVarName).
               Write(call.Args).
               EndCall(inline);
    }

    ProgramWriter Write(Expression[] args)
    {
        if (args != null)
        {
            for (int i = 0; i < args.Length; ++i)
            {
                if (i > 0) { Write(", "); }
                Write(args[i]);
            }
        }
        return this;
    }

    ProgramWriter Write(Expression expr)
    {
        switch (expr)
        {
            default:
                Write($"/* {expr.Source.Stringify()} */");
                break;
            case FunctionCall call:
                Write(call, true);
                break;
            case ResultReference resultRef:
                Write(ResultVar(resultRef.Ref));
                break;
            case ValueExpr valueExpr:
                Write(valueExpr.Value.Stringify());
                break;
            case ArrayExpr arrayExpr:
                Write("[").Write(arrayExpr.Value).Write("]");
                break;
            case ObjectExpr objectExpr:
                Write("/*");
                Write("{");
                int i = 0;
                foreach (var property in objectExpr.Value)
                {
                    Write(property.Key.Stringify()).Write(": ");
                    Write(property.Value);
                    if (i > 0) { Write(", "); }
                    ++i;
                }
                Write("}");
                Write("*/");
                break;
        }
        return this;
    }

    ProgramWriter BeginCall(string name, string api = null)
    {
        return Write(api != null ? $"{api}.{name}(" : $"{name}(");
    }
    string ResultVar(int resultNumber) => (ResultVarPrefix + (resultNumber + 1));

    ProgramWriter EndCall(bool inline = false) => Write(inline ? ")" : ");").EOL();
    ProgramWriter SOL() { _writer.SOL(); return this; }
    ProgramWriter EOL() { _writer.EOL(); return this; }
    ProgramWriter Write(char ch) { _writer.Append(ch); return this; }
    ProgramWriter Write(string value) { _writer.Append(value); return this; }
}
