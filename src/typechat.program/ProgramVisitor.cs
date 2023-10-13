// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// Simple helper class for traversing a Program's AST
/// </summary>
public class ProgramVisitor
{
    public ProgramVisitor() { }

    /// <summary>
    /// Visits the Program Recursively
    /// </summary>
    /// <param name="program"></param>
    public void Visit(Program program)
    {
        if (program.Steps is not null)
        {
            VisitSteps(program.Steps);
        }
    }

    protected virtual void VisitSteps(Steps steps)
    {
        for (int i = 0; i < steps.Calls.Length; ++i)
        {
            VisitStep(steps.Calls[i], i);
        }
    }

    protected virtual void VisitStep(FunctionCall function, int stepNumber)
    {
        VisitFunction(function);
    }

    protected virtual void VisitFunction(FunctionCall functionCall)
    {
        Visit(functionCall.Args);
    }

    protected virtual void VisitResult(ResultReference resultRef) { }

    protected virtual void VisitValue(ValueExpr value) { }

    protected virtual void VisitArray(ArrayExpr array) => Visit(array.Value);

    protected virtual void VisitObject(ObjectExpr obj)
    {
        foreach (var property in obj.Value)
        {
            Visit(property.Value);
        }
    }

    protected void Visit(Expression[] expressions)
    {
        foreach (Expression expression in expressions ?? Array.Empty<Expression>())
        {
            Visit(expression);
        }
    }

    protected void Visit(Expression expr)
    {
        switch (expr)
        {
            default:
                break;

            case FunctionCall call:
                VisitFunction(call);
                break;

            case ResultReference result:
                VisitResult(result);
                break;

            case ValueExpr value:
                VisitValue(value);
                break;

            case ArrayExpr array:
                VisitArray(array);
                break;

            case ObjectExpr obj:
                VisitObject(obj);
                break;
        }
    }
}
