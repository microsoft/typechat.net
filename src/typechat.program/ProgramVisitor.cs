// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public class ProgramVisitor
{
    public ProgramVisitor() { }

    public void Visit(Program program)
    {
        if (program.Steps != null)
        {
            VisitSteps(program.Steps);
        }
    }

    protected virtual void VisitSteps(Steps steps)
    {
        for (int i = 0; i < steps.Calls.Length; ++i)
        {
            VisitFunction(steps.Calls[i]);
        }
    }

    protected virtual void VisitFunction(FunctionCall functionCall)
    {
        Visit(functionCall.Args);
    }

    protected virtual void VisitResult(ResultReference resultRef) {}
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
        if (expressions == null || expressions.Length == 0)
        {
            return;
        }
        for (int i = 0; i < expressions.Length; ++i)
        {
            Visit(expressions[i]);
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
