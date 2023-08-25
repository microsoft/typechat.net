// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

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

public abstract partial class Expression
{
}

public partial class Steps : Expression
{
    public FunctionCall[] Calls
    {
        get;
        private set;
    }
}

public partial class FunctionCall : Expression
{
    public string Name
    {
        get;
        private set;
    }

    public Expression[] Args
    {
        get;
        private set;
    }
}

public partial class ResultReference : Expression
{
    [Comment("Index of the previous expression in the \"@steps\" array")]
    public int Ref { get; set; }
}

public partial class ValueExpr : Expression
{
    public JsonElement Value
    {
        get;
        private set;
    }
}

public partial class ArrayExpr : Expression
{
    public Expression[] Value
    {
        get;
        private set;
    }
}

public partial class ObjectExpr : Expression
{
    public Dictionary<string, Expression> Value
    {
        get;
        private set;
    }
}

public partial class NotTranslatedExpr : Expression
{
    public string Text { get; set; }
}

public partial class UnknownExpr : Expression { }
