// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel;

namespace Microsoft.TypeChat;

/// <summary>
/// Validates programs produced by PluginProgramTranslator.
/// Ensures that function calls are to existing plugins with matching parameters
/// </summary>
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

    protected override void VisitFunction(FunctionCall functionCall)
    {
        try
        {
            // Verify function exists
            var name = PluginFunctionName.Parse(functionCall.Name);
            KernelFunctionMetadata typeInfo = _typeInfo[name];

            // Verify that parameter counts etc match
            ValidateArgs(functionCall, typeInfo, functionCall.Args);

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

    void ValidateArgs(FunctionCall call, KernelFunctionMetadata typeInfo, Expression[] args)
    {
        // Verify arg counts
        CheckArgCount(call, typeInfo, args);

        // Verify arg types
        TypeCheckArgs(call, typeInfo.Parameters, args);
    }

    int GetRequiredArgCount(IReadOnlyList<KernelParameterMetadata> parameters)
    {
        int requiredCount = 0;

        for (int i = 0; i < parameters.Count; ++i)
        {
            if (IsOptional(parameters[i]))
            {
                // Optional parameters follow required ones
                break;
            }

            requiredCount++;
        }

        return requiredCount;
    }

    void CheckArgCount(FunctionCall call, KernelFunctionMetadata typeInfo, Expression[] args)
    {
        // Just checks if the right number of parameters were supplied
        int requiredArgCount = (typeInfo.Parameters is not null) ? GetRequiredArgCount(typeInfo.Parameters) : 0;
        int actualCount = (args is not null) ? args.Length : 0;

        if (actualCount < requiredArgCount)
        {
            ProgramException.ThrowArgCountMismatch(call, requiredArgCount, actualCount);
        }

        int totalArgCount = (typeInfo.Parameters is not null) ? typeInfo.Parameters.Count : 0;
        if (actualCount > totalArgCount)
        {
            ProgramException.ThrowArgCountMismatch(call, totalArgCount, actualCount);
        }
    }

    void TypeCheckArgs(FunctionCall call, IReadOnlyList<KernelParameterMetadata> parameters, Expression[] args)
    {
        Debug.Assert(args.Length <= parameters.Count);

        for (int i = 0; i < args.Length; ++i)
        {
            Type expectedType = parameters[i].ParameterType;
            if (expectedType is not null)
            {
                Type exprType = ParameterTypeFromExpr(args[i]);
                if (expectedType != exprType && exprType != typeof(object))
                {
                    ThrowTypeMismatch(call, parameters[i].Name, expectedType, exprType);
                }
            }
        }
    }

    Type ParameterTypeFromExpr(Expression expr)
    {
        switch (expr.ValueType)
        {
            default:
                return typeof(string);

            case JsonValueKind.True:
            case JsonValueKind.False:
                return typeof(bool);

            case JsonValueKind.String:
                return typeof(string);

            case JsonValueKind.Number:
                return typeof(double);

            case JsonValueKind.Array:
                return typeof(Array);

            case JsonValueKind.Object:
                return typeof(object);
        }
    }

    bool IsOptional(KernelParameterMetadata parameter)
    {
        return parameter.DefaultValue is null;
    }

    void ThrowTypeMismatch(FunctionCall call, string paramName, Type expectedType, Type actualType)
    {
        throw new ProgramException(
            ProgramException.ErrorCode.TypeMismatch,
            $"TypeMismatch: @func {call.Name} @arg {paramName}: Expected {expectedType.Name}, Got {actualType.Name}"
            );

    }
}
