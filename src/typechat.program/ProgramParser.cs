// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public class ProgramParser
{
    public class ExprNames
    {
        public const string Steps = "@steps";
        public const string Func = "@func";
        public const string Args = "@args";
        public const string Ref = "@ref";
    }

    public ProgramParser() { }

    public Program Parse(string programSource)
    {
        return Parse(JsonDocument.Parse(programSource));
    }

    public Program Parse(JsonDocument programSource)
    {
        JsonElement root = programSource.RootElement;
        root.EnsureIsType(JsonValueKind.Object);
        Expr expr = ParseObject(root);
        if (expr is Steps steps)
        {
            return new Program(programSource, steps);
        }
        throw new ProgramException(ProgramException.ErrorCode.InvalidProgram, root.ToString());
    }

    Steps ParseSteps(JsonElement source)
    {
        Debug.Assert(source.ValueKind == JsonValueKind.Array);
        Call[] steps = new Call[source.GetArrayLength()];
        for (int i = 0; i < steps.Length; ++i)
        {
            steps[i] = ParseCall(source[i]);
        }
        return new Steps(source, steps);
    }

    Call ParseCall(JsonElement elt)
    {
        return ParseCall(elt, elt.GetStringProperty(ExprNames.Func));
    }

    Call ParseCall(JsonElement source, JsonElement funcName)
    {
        Debug.Assert(source.ValueKind == JsonValueKind.Object);
        Expr[] args = ParseArgs(source);
        return new Call(source, funcName, args);
    }

    public Expr[] ParseArgs(JsonElement elt)
    {
        if (!elt.TryGetProperty(ExprNames.Args, out JsonElement args))
        {
            return Expr.Empty;
        }
        args.EnsureIsType(JsonValueKind.Array, ExprNames.Args);
        return ParseExprArray(args);
    }

    Expr[] ParseExprArray(JsonElement elt)
    {
        Debug.Assert(elt.ValueKind == JsonValueKind.Array);
        Expr[] expr = new Expr[elt.GetArrayLength()];
        for (int i = 0; i < expr.Length; ++i)
        {
            expr[i] = ParseExpr(elt[i]);
        }
        return expr;
    }

    Expr ParseObject(JsonElement elt)
    {
        Debug.Assert(elt.ValueKind == JsonValueKind.Object);

        if (elt.TryGetProperty(ExprNames.Func, out JsonElement funcName))
        {
            funcName.EnsureIsType(JsonValueKind.String, ExprNames.Func);
            return ParseCall(elt, funcName);
        }
        else if (elt.TryGetProperty(ExprNames.Ref, out JsonElement refValue))
        {
            refValue.EnsureIsType(JsonValueKind.Number, ExprNames.Ref);
            return new ResultRef(elt, refValue);
        }
        else if (elt.TryGetProperty(ExprNames.Steps, out JsonElement steps))
        {
            steps.EnsureIsType(JsonValueKind.Array, ExprNames.Steps);
            return ParseSteps(steps);
        }
        return new UnknownExpr(elt);
    }

    Expr ParseExpr(JsonElement elt)
    {
        switch (elt.ValueKind)
        {
            default:
                break;

            case JsonValueKind.Object:
                return ParseObject(elt);

            case JsonValueKind.Null:
            case JsonValueKind.Number:
            case JsonValueKind.String:
            case JsonValueKind.True:
            case JsonValueKind.False:
                return new ValueExpr(elt);

            case JsonValueKind.Array:
                return new ArrayExpr(elt, ParseExprArray(elt));
        }

        return new UnknownExpr(elt);
    }
}
