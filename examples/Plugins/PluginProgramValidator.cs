// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel.SkillDefinition;
using Microsoft.TypeChat;

namespace Plugins;

public class PluginProgramValidator : ProgramVisitor, IProgramValidator
{
    PluginApiTypeInfo _typeInfo;

    public PluginProgramValidator(PluginApiTypeInfo typeInfo)
    {
        _typeInfo = typeInfo;
    }

    public ValidationResult<Program> ValidateProgram(Program program)
    {
        try
        {
            Visit(program);
            return program;
        }
        catch (Exception ex)
        {
            return ValidationResult<Program>.Error(ex.Message);
        }
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
            // Continue visiting
            base.VisitFunction(functionCall);
            return;
        }
        catch(ProgramException)
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
