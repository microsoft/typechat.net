﻿// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// Parses a Json Program into a Program object
/// </summary>
public class ProgramParser
{
    public class ExprNames
    {
        public const string Steps = "@steps";
        public const string Func = "@func";
        public const string Args = "@args";
        public const string Ref = "@ref";
        public const string CannotTranslate = "@cannot_translate";
    }

    /// <summary>
    /// Create a new parser
    /// </summary>
    public ProgramParser() { }

    /// <summary>
    /// Parse the given program source
    /// </summary>
    /// <param name="programSource"></param>
    /// <returns>parsed program</returns>
    public Program Parse(string programSource)
    {
        return Parse(JsonDocument.Parse(programSource));
    }

    /// <summary>
    /// Parsed the given program source found in a Json document
    /// </summary>
    /// <param name="programSource"></param>
    /// <returns>parsed program</returns>
    /// <exception cref="ProgramException"></exception>
    public Program Parse(JsonDocument programSource)
    {
        JsonElement root = programSource.RootElement;
        root.EnsureIsType(JsonValueKind.Object);

        Program program = new Program(programSource);

        if (root.TryGetProperty(ExprNames.CannotTranslate, out JsonElement cannotElt))
        {
            cannotElt.EnsureIsType(JsonValueKind.Array, ExprNames.CannotTranslate);
            program.NotTranslated = ParseNotTranslated(cannotElt);
        }
        if (root.TryGetProperty(ExprNames.Steps, out JsonElement stepsElt))
        {
            stepsElt.EnsureIsType(JsonValueKind.Array, ExprNames.Steps);
            try
            {
                program.Steps = ParseSteps(stepsElt);
            }
            catch (JsonException)
            {
                // If the LLM returned not translated parts, then it cannot translate the user's
                // request into JSON. Any other parse errors can be ignored
                if (!program.HasNotTranslated)
                {
                    throw;
                }
            }
        }

        if (program.HasSteps || program.HasNotTranslated)
        {
            // Something useful to return back to the caller
            return program;
        }

        throw new ProgramException(ProgramException.ErrorCode.InvalidProgramJson, root.ToString());
    }

    private Steps ParseSteps(JsonElement source)
    {
        Debug.Assert(source.ValueKind == JsonValueKind.Array);
        FunctionCall[] steps = new FunctionCall[source.GetArrayLength()];
        for (int i = 0; i < steps.Length; ++i)
        {
            steps[i] = ParseCall(source[i]);
        }
        return new Steps(source, steps);
    }

    private string[] ParseNotTranslated(JsonElement source)
    {
        Debug.Assert(source.ValueKind == JsonValueKind.Array);
        try
        {
            string[] items = new string[source.GetArrayLength()];
            for (int i = 0; i < items.Length; ++i)
            {
                items[i] = source[i].GetString();
            }
            return items;
        }
        catch { }
        return null;
    }

    private FunctionCall ParseCall(JsonElement elt)
    {
        return ParseCall(elt, elt.GetStringProperty(ExprNames.Func));
    }

    private FunctionCall ParseCall(JsonElement source, JsonElement funcName)
    {
        Debug.Assert(source.ValueKind == JsonValueKind.Object);
        Expression[] args = ParseArgs(source);
        return new FunctionCall(source, funcName, args);
    }

    public Expression[] ParseArgs(JsonElement elt)
    {
        if (!elt.TryGetProperty(ExprNames.Args, out JsonElement args))
        {
            return Expression.Empty;
        }
        args.EnsureIsType(JsonValueKind.Array, ExprNames.Args);
        return ParseExprArray(args);
    }

    private Expression[] ParseExprArray(JsonElement elt)
    {
        Debug.Assert(elt.ValueKind == JsonValueKind.Array);
        Expression[] expr = new Expression[elt.GetArrayLength()];
        for (int i = 0; i < expr.Length; ++i)
        {
            expr[i] = ParseExpr(elt[i]);
        }
        return expr;
    }

    private Expression ParseObject(JsonElement elt)
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
            return new ResultReference(elt, refValue);
        }
        else
        {
            // Parse as generic object
            return ParseObjectExpr(elt);
        }
    }

    private ObjectExpr ParseObjectExpr(JsonElement elt)
    {
        Debug.Assert(elt.ValueKind == JsonValueKind.Object);
        Dictionary<string, Expression> obj = new Dictionary<string, Expression>();
        foreach (var property in elt.EnumerateObject())
        {
            obj[property.Name] = ParseExpr(property.Value);
        }

        return new ObjectExpr(elt, obj);
    }

    private Expression ParseExpr(JsonElement elt)
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
