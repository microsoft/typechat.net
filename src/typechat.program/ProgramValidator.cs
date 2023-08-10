// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// (Experimental) Lightweight program validator
/// Traverses the supplied program and validates that it is Type Safe
/// ObjectExpr is currently not supported
/// </summary>
public class ProgramValidator
{
    ApiTypeInfo _typeInfo;
    Steps _steps;

    public ProgramValidator(Type type)
        : this(new ApiTypeInfo(type))
    {
    }

    public ProgramValidator(ApiTypeInfo typeInfo)
    {
        ArgumentNullException.ThrowIfNull(typeInfo, nameof(typeInfo));
        _typeInfo = typeInfo;
    }

    public void Validate(Program program)
    {
        ArgumentNullException.ThrowIfNull(program, nameof(program));

        Clear();
        Validate(program.Steps);
    }

    void Clear()
    {
        _steps = null;
    }

    void Validate(Steps steps)
    {
        _steps = steps;
        for (int i = 0; i < _steps.Calls.Length; ++i)
        {
            FunctionCall call = _steps.Calls[i];
            ApiMethod method = _typeInfo[call.Name];
            Validate(call, method.ReturnType.ParameterType);
        }
    }

    void Validate(FunctionCall call, Expression expr, Type expectedType)
    {
        switch (expr)
        {
            default:
                break;
            case FunctionCall funcExpr:
                Validate(funcExpr, expectedType);
                return;
            case ArrayExpr arrayExpr:
                Validate(call, arrayExpr, expectedType);
                return;
            case ResultReference resultRef:
                Validate(call, resultRef, expectedType);
                return;
        }
        if (expr.Type != expectedType && expectedType != typeof(object))
        {
            ProgramException.ThrowTypeMismatch(call.Name, expectedType, expr.Type);
        }
    }

    void Validate(FunctionCall call, ArrayExpr expr, Type expectedType)
    {
        if (!expectedType.IsArray)
        {
            ProgramException.ThrowTypeMismatch(expr.Type, expectedType);
        }
        Type itemType = expectedType.GetElementType();
        Expression[] arrayItems = expr.Value;
        for (int i = 0; i < arrayItems.Length; ++i)
        {
            Validate(call, arrayItems[i], itemType);
        }
    }

    void Validate(FunctionCall call, ResultReference resultRef, Type expectedType)
    {
        if (resultRef.Ref < 0 ||
            resultRef.Ref >= _steps.Calls.Length)
        {
            ProgramException.ThrowInvalidResultRef(resultRef.Ref);
        }
        ApiMethod apiMethod = _typeInfo[_steps.Calls[resultRef.Ref].Name];
        Type returnType = apiMethod.ReturnType.ParameterType;
        if (returnType != expectedType)
        {
            ProgramException.ThrowTypeMismatch(call.Name, expectedType, returnType);
        }
    }

    void Validate(FunctionCall call, Type expectedReturnType)
    {
        ApiMethod apiMethod = _typeInfo[call.Name];
        Type returnType = apiMethod.ReturnType.ParameterType;
        if (returnType != expectedReturnType)
        {
            ProgramException.ThrowTypeMismatch(call.Name, expectedReturnType, returnType);
        }
        //
        // Recursively validate the arguments
        //
        Expression[] args = call.Args;
        if (apiMethod.Params.Length == 1 &&
            args.Length > 1)
        {
            Type paramType = apiMethod.Params[0].ParameterType;
            if (!paramType.IsArray)
            {
                ProgramException.ThrowTypeMismatch(call.Name, paramType, expectedReturnType);
            }
            for (int i = 0; i < args.Length; ++i)
            {
                Validate(call, args[i], paramType.GetElementType());
            }
            return;
        }

        if (args.Length != apiMethod.Params.Length)
        {
            ProgramException.ThrowArgCountMismatch(call.Name, apiMethod.Params.Length, args.Length);
        }
        for (int i = 0; i < args.Length; ++i)
        {
            Validate(call, args[i], apiMethod.Params[i].ParameterType);
        }
    }
}
