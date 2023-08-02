// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public partial class Program : IDisposable
{
    JsonDocument? _programSource;

    public Program(JsonDocument? source, Steps steps)
    {
        ArgumentNullException.ThrowIfNull(steps, nameof(steps));
        _programSource = source;
        Steps = steps;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool fromDispose)
    {
        if (fromDispose)
        {
            _programSource?.Dispose();
            _programSource = null;
        }
    }
}

public abstract partial class Expr
{
    internal static readonly Expr[] Empty = new Expr[0];

    public Expr(JsonElement source)
    {
        Source = source;
    }
}

public partial class Steps : Expr
{
    static readonly Call[] EmptySteps = new Call[0];

    public Steps(JsonElement source, Call[]? calls)
        : base(source)
    {
        calls ??= EmptySteps;
        Calls = calls;
    }
}

public partial class Call : Expr
{
    public Call(JsonElement source, JsonElement name, Expr[] args)
        : base(source)
    {
        Debug.Assert(name.ValueKind == JsonValueKind.String);
        Name = name.GetString();
        Args = args;
    }
}

public partial class ResultRef : Expr
{
    public ResultRef(JsonElement source, JsonElement value)
        : base(source)
    {
        Debug.Assert(value.ValueKind == JsonValueKind.Number);
        Ref = value.GetInt32();
        if (Ref < 0)
        {
            throw new ProgramException(ProgramException.ErrorCode.InvalidResultRef, $"{Ref} ins not a valid ref");
        }
    }
}

public partial class ValueExpr : Expr
{
    public ValueExpr(JsonElement source)
        : base(source)
    {
        Value = source;
    }
}

public partial class ArrayExpr : Expr
{
    public ArrayExpr(JsonElement source, Expr[] exprs)
        : base(source)
    {
        Value = exprs;
    }
}

public partial class UnknownExpr : Expr
{
    public UnknownExpr(JsonElement source)
        : base(source)
    {
    }
}
