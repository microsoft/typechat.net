// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// A simple and fast program validator that ensures that any function calls
/// are to actual APIs with the parameter types that match
/// </summary>
public class FunctionCallValidator<TApi> : ProgramVisitor, IProgramValidator
{
    Api<TApi> _api;

    public FunctionCallValidator(Api<TApi> api)
        : base()
    {
        _api = api;
    }

    /// <summary>
    /// Validate the program
    /// If successful, returns Result.Success
    /// Else returns Result with Success false, along with a diagnostic message
    /// Currently, the validator stops when it hits its first error.
    /// Future versions will support collecting all diagnostics before returning
    /// </summary>
    /// <param name="program">program to validate</param>
    /// <returns></returns>
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
        int expectedCount = methodInfo.Params != null ? methodInfo.Params.Length : 0;
        int actualCount = args != null ? args.Length : 0;
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
