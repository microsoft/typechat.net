// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

[Comment("A program consists of a sequence of function calls that are evaluated in order")]
public class Program
{
    [JsonPropertyName("steps")]
    public Call[] Steps { get; set; }
}

[Comment("An expression is a JSON value, a function call, or a reference to the result of a preceding expression.")]
[JsonPolymorphic]
[JsonDerivedType(typeof(Call), typeDiscriminator: nameof(Call))]
[JsonDerivedType(typeof(ResultRef), typeDiscriminator: nameof(ResultRef))]
[JsonDerivedType(typeof(Value), typeDiscriminator: nameof(Value))]
public abstract class Expr { }

[Comment("Value are ONE of the fo")]
[Comment("type Json Value = string | number | boolean | null | Expr[]")]
public class Value : Expr
{
    public enum Type
    {
        Null,
        String,
        Number,
        Bool,
        Array
    }

    public string? String { get; set; }
    public double? Number { get; set; }
    public bool? Bool { get; set; }
    public Expr[]? Array { get; set; }
}

[Comment("A function call specifies a function name and a list of argument expressions.")]
[Comment("Arguments may contain nested function calls and result references.")]
public class Call : Expr
{
    // Name of the function
    [JsonPropertyName("func")]
    public string Func { get; set; }
    // Arguments for the function, if any
    [JsonPropertyName("args")]
    public Expr[]? Args { get; set; }
};

public class ResultRef : Expr
{
    // Index of the previous expression in the "@steps" array
    [JsonPropertyName("ref")]
    public int Ref { get; set; }
}
