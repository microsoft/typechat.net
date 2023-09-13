// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel.SkillDefinition;
using Microsoft.TypeChat.CSharp;
using Microsoft.TypeChat.Schema;

namespace Microsoft.TypeChat;

public class PluginProgramValidator : ProgramVisitor, IProgramValidator
{
    PluginApiTypeInfo _typeInfo;

    public PluginProgramValidator(PluginApiTypeInfo typeInfo)
    {
        _typeInfo = typeInfo;
    }

    public Result<Program> ValidateProgram(Program program)
    {
        try
        {
            Visit(program);
            return program;
        }
        catch (Exception ex)
        {
            return Result<Program>.Error(program, ex.Message);
        }
    }

    public void CSharpValidate(Program program)
    {
        using StringWriter sw = new StringWriter();
        new ProgramWriter(sw).Write(program, "IPlugin");
        CSharpProgramCompiler compiler = new CSharpProgramCompiler();
        compiler.AddCode(sw.ToString());
        string diagnostics = compiler.GetDiagnostics();
    }

    protected override void VisitFunction(FunctionCall functionCall)
    {
        try
        {
            // Verify function exists
            var name = PluginFunctionName.Parse(functionCall.Name);
            FunctionView typeInfo = _typeInfo[name];
            // Verify that parameter counts etc match
            ValidateArgCounts(functionCall, typeInfo, functionCall.Args);
            // Continue visiting to handle any nested function calls
            base.VisitFunction(functionCall);
            return;
        }
        catch (ProgramException)
        {
            throw;
        }
        catch { }
        ProgramException.ThrowFunctionNotFound(functionCall.Name);
    }

    void ValidateArgCounts(FunctionCall call, FunctionView typeInfo, Expression[] args)
    {
        int expectedCount = (typeInfo.Parameters != null) ? typeInfo.Parameters.Count : 0;
        int actualCount = (args != null) ? args.Length : 0;
        if (actualCount != expectedCount)
        {
            ProgramException.ThrowArgCountMismatch(call, expectedCount, actualCount);
        }
    }
}
