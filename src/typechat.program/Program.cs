// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

[JsonConverter(typeof(JsonProgramConvertor))]
public partial class Program
{
    public Steps Steps
    {
        get;
        private set;
    }
}

public abstract partial class Expr
{
    public JsonElement Source
    {
        get;
        private set;
    }
}

public partial class Steps : Expr
{
    public Call[] Calls
    {
        get;
        private set;
    }
}

public partial class Call : Expr
{
    public string Name
    {
        get;
        private set;
    }

    public Expr[] Args
    {
        get;
        private set;
    }
}

public partial class ResultRef : Expr
{
    [Comment("Index of the previous expression in the \"@steps\" array")]
    public int Ref { get; set; }
}

public partial class ValueExpr : Expr
{
    public JsonElement Value
    {
        get;
        private set;
    }
}

public partial class ArrayExpr : Expr
{
    public Expr[] Value
    {
        get;
        private set;
    }
}

public partial class UnknownExpr : Expr
{

}
