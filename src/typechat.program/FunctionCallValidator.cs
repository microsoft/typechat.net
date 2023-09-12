// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

/// <summary>
/// A simple and fast program validator that ensures that any function calls
/// are to actual APIs with the right parameters
/// </summary>
public class FunctionCallValidator<TApi> : ProgramVisitor, IProgramValidator
{
    Api<TApi> _api;

    public FunctionCallValidator(Api<TApi> api)
        : base()
    {
        _api = api;
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

    protected override void VisitFunction(FunctionCall functionCall)
    {
        try
        {
            // Verify function exists
            ApiMethod method = _api.TypeInfo[functionCall.Name];
            // Verify that parameter counts etc match
            ValidateArgs(functionCall, method, functionCall.Args);
            // Continue visiting to handle any nested calls
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

    void ValidateArgs(FunctionCall call, ApiMethod methodInfo, Expression[] args)
    {
        int expectedCount = (methodInfo.Params != null) ? methodInfo.Params.Length : 0;
        int actualCount = (args != null) ? args.Length : 0;
        if (actualCount != expectedCount)
        {
            ProgramException.ThrowArgCountMismatch(call, expectedCount, actualCount);
        }
        for (int i = 0; i < args.Length; ++i)
        {
            Type actualType = args[i].Type;
            if (!methodInfo.Params[i].IsMatchingType(actualType))
            {
                ProgramException.ThrowTypeMismatch(call, methodInfo.Params[i], actualType);
            }
        }
    }
}
