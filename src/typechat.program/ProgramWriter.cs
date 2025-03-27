// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// Simple Program writer: writes out Program as a pseudo C# like function
/// Useful for displaying programs to a user
/// </summary>
public class ProgramWriter
{
    private CodeWriter _writer;

    public ProgramWriter(TextWriter writer)
    {
        _writer = new CodeWriter(writer);
    }

    private string ProgramName { get; set; } = "Program";
    private string ApiVarName { get; set; } = "api";
    private string ResultVarPrefix { get; set; } = "step";

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

    public ProgramWriter Write(Program program, Type apiType) => Write(program, apiType.Name);

    public ProgramWriter Write(Program program, string apiName)
    {
        ArgumentVerify.ThrowIfNull(program, nameof(program));

        if (program.Steps is null || program.Steps.Calls.IsNullOrEmpty())
        {
            return this;
        }

        SOL().Write($"dynamic {ProgramName}({apiName} {ApiVarName}) {{").EOL();

        _writer.PushIndent();
        Write(program.Steps).Write("}").EOL();
        _writer.PopIndent();

        return this;
    }

    private ProgramWriter Write(Steps steps)
    {
        FunctionCall[] calls = steps.Calls;
        if (calls is not null && calls.Length > 0)
        {
            for (int i = 0; i < calls.Length; ++i)
            {
                _writer.SOL().Append($"var {ResultVar(i)} = ");
                Write(calls[i]);
            }
            _writer.SOL().Append($"return {ResultVar(calls.Length - 1)};").EOL();
        }
        return this;
    }

    private ProgramWriter Write(FunctionCall call, bool inline = false)
    {
        return BeginCall(call.Name, ApiVarName).
               Write(call.Args).
               EndCall(inline);
    }

    private ProgramWriter Write(Expression[] args)
    {
        if (args is not null)
        {
            for (int i = 0; i < args.Length; ++i)
            {
                if (i > 0) { Write(", "); }
                Write(args[i]);
            }
        }
        return this;
    }

    private ProgramWriter Write(Expression expr)
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

    private ProgramWriter BeginCall(string name, string api = null)
    {
        return Write(api is not null ? $"{api}.{name}(" : $"{name}(");
    }

    private string ResultVar(int resultNumber) => (ResultVarPrefix + (resultNumber + 1));

    private ProgramWriter EndCall(bool inline = false)
    {
        if (inline)
        {
            Write(")");
        }
        else
        {
            Write(");").EOL();
        }
        return this;
    }

    private ProgramWriter SOL() { _writer.SOL(); return this; }
    private ProgramWriter EOL() { _writer.EOL(); return this; }
    private ProgramWriter Write(char ch) { _writer.Append(ch); return this; }
    private ProgramWriter Write(string value) { _writer.Append(value); return this; }
}
