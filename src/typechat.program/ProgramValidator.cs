// Copyright (c) Microsoft. All rights reserved.

using System.Reflection;
using System.Text.Json;

namespace Microsoft.TypeChat;

public class ProgramValidator<T>
{
    ApiTypeInfo _typeInfo;

    public ProgramValidator(ApiTypeInfo typeInfo)
    {
        ArgumentNullException.ThrowIfNull(typeInfo, nameof(typeInfo));
        _typeInfo = typeInfo;
    }

    public void ValidateFunction(FunctionCall call)
    {
        ArgumentNullException.ThrowIfNull(call, nameof(call));

        ApiMethod method = _typeInfo[call.Name];
        ValidateParams(call, method.Params);
    }

    void ValidateParams(FunctionCall call, ParameterInfo[] parameters)
    {
        ValidateParameterCount(call, parameters);
        for (int i = 0; i < parameters.Length; ++i)
        {
            ValidateParam(call, call.Args[i], parameters[i]);
        }
    }

    int ValidateParameterCount(FunctionCall call, ParameterInfo[] parameters)
    {
        Expression[] expressions = call.Args;
        int exprCount = (expressions != null) ? expressions.Length : 0;
        int paramCount = (parameters != null) ? parameters.Length : 0;
        if (exprCount != paramCount)
        {
            ProgramException.ThrowArgCountMismatch(call.Name, paramCount, exprCount);
        }
        return paramCount;
    }

    void ValidateParam(FunctionCall call, Expression expr, ParameterInfo param)
    {
        Type paramType = param.ParameterType;
        JsonValueKind exprType = expr.ValueType;
        if (expr.ValueType == JsonValueKind.Undefined)
        {
            // We don't validate this yet. Will happen at runtime
            return;
        }

        // Very basic validation
        var (isType, expectedType) = exprType.IsType(paramType);
        if (!isType)
        {
            ProgramException.ThrowTypeMismatch(exprType, expectedType);
        }
    }
}
