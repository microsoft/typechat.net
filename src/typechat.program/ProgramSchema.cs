// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// A Json Program. See ProgramSchema.ts for the grammar for Programs
/// This class is the root is a simple AST for programs
/// </summary>
[JsonConverter(typeof(JsonProgramConvertor))]
public partial class Program
{
    /// <summary>
    /// Generated steps to take for this program
    /// </summary>
    public Steps? Steps
    {
        get;
        internal set;
    }

    /// <summary>
    /// Part or all of the user request that could not be translated into program steps
    /// </summary>
    public string[] NotTranslated { get; set; }
}

/// <summary>
/// Base class for Expressions
/// </summary>
public abstract partial class Expression
{
}

/// <summary>
/// A program contains steps implemented as function calls
/// </summary>
public partial class Steps : Expression
{
    public FunctionCall[] Calls
    {
        get;
        private set;
    }
}

/// <summary>
/// A Function Call to a named function with zero or more arguments
/// </summary>
public partial class FunctionCall : Expression
{
    /// <summary>
    /// Function name
    /// </summary>
    public string Name
    {
        get;
        private set;
    }

    /// <summary>
    /// Arguments to pass this function.
    /// Arguments can be any Expression, including nested function calls.
    /// </summary>
    public Expression[] Args
    {
        get;
        private set;
    }
}

/// <summary>
/// A reference to result of prior step in the program
/// </summary>
public partial class ResultReference : Expression
{
    [Comment("Index of the previous expression in the \"@steps\" array")]
    public int Ref { get; set; }
}

/// <summary>
/// An expression representing a Json value
/// </summary>
public partial class ValueExpr : Expression
{
    public JsonElement Value
    {
        get;
        private set;
    }
}

/// <summary>
/// An array expression - can contain any expression including function calls
/// </summary>
public partial class ArrayExpr : Expression
{
    public Expression[] Value
    {
        get;
        private set;
    }
}

/// <summary>
/// A Json Object. 
/// </summary>
public partial class ObjectExpr : Expression
{
    /// <summary>
    /// Properties for this object
    /// </summary>
    public Dictionary<string, Expression> Value
    {
        get;
        private set;
    }
}

/// <summary>
/// Parts of the user request that could not be translated into a program
/// </summary>
public partial class NotTranslatedExpr : Expression
{
    public string Text { get; set; }
}

/// <summary>
/// The model returned something we don't recognize
/// </summary>
public partial class UnknownExpr : Expression { }
