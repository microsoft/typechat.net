// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public class Program
{
    [JsonPropertyName("@steps")]
    public FunctionCall[] Steps { get; set; }
}

/**
 * An expression is a JSON value, a function call, or a reference to the result of a preceding expression.
 */
public abstract class Expression { }

public class JsonValue : Expression { }
public class StringValue : JsonValue { }
public class NumberValue : JsonValue { }
public class BooleanValue : JsonValue { }
public class ArrayValue : Expression { }

/**
 * A function call specifices a function name and a list of argument expressions. Arguments may contain
 * nested function calls and result references.
 */
public class FunctionCall : Expression
{
    // Name of the function
    [JsonPropertyName("@func")]
    public string Func { get; set; }
    // Arguments for the function, if any
    [JsonPropertyName("@args")]
    public Expression[]? Args { get; set; }
};

public class ResultReference : Expression
{
    // Index of the previous expression in the "@steps" array
    [JsonPropertyName("@ref")]
    public int Ref { get; set; }
}
