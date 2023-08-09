﻿// Copyright (c) Microsoft. All rights reserved.

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

public abstract partial class Expression
{
    internal static readonly Expression[] Empty = new Expression[0];

    public Expression(JsonElement source)
    {
        Source = source;
    }

    [JsonIgnore]
    public virtual JsonValueKind ValueType => Source.ValueKind;
    [JsonIgnore]
    internal virtual Type Type => typeof(object);
}

public partial class Steps : Expression
{
    static readonly FunctionCall[] EmptySteps = new FunctionCall[0];

    public Steps(JsonElement source, FunctionCall[]? calls)
        : base(source)
    {
        calls ??= EmptySteps;
        Calls = calls;
    }
}

public partial class FunctionCall : Expression
{
    public FunctionCall(JsonElement source, JsonElement name, Expression[] args)
        : base(source)
    {
        Debug.Assert(name.ValueKind == JsonValueKind.String);
        Name = name.GetString();
        Args = args;
    }

    // Undefined for now. Until we walk the expression tree
    public override JsonValueKind ValueType => JsonValueKind.Undefined;

    public override string ToString()
    {
        return Name;
    }
}

public partial class ResultReference : Expression
{
    public ResultReference(JsonElement source, JsonElement value)
        : base(source)
    {
        Debug.Assert(value.ValueKind == JsonValueKind.Number);
        Ref = value.GetInt32();
        if (Ref < 0)
        {
            ProgramException.ThrowInvalidResultRef(Ref);
        }
    }
}

public partial class ValueExpr : Expression
{
    public ValueExpr(JsonElement source)
        : base(source)
    {
        Value = source;
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    internal override Type Type
    {
        get
        {
            switch(ValueType)
            {
                default:
                    break;
                case JsonValueKind.String:
                    return typeof(string);
                case JsonValueKind.Number:
                    return typeof(double);
                case JsonValueKind.True:
                case JsonValueKind.False:
                    return typeof(bool);
            }
            return base.Type;
        }
    }
}

public partial class ArrayExpr : Expression
{
    public ArrayExpr(JsonElement source, Expression[] exprs)
        : base(source)
    {
        ArgumentNullException.ThrowIfNull(exprs, nameof(exprs));
        Value = exprs;
    }
}

public partial class ObjectExpr : Expression
{
    public ObjectExpr(JsonElement source, Dictionary<string, Expression> obj)
        : base(source)
    {
        ArgumentNullException.ThrowIfNull(obj, nameof(obj));
        Value = obj;
    }
}

public partial class UnknownExpr : Expression
{
    public UnknownExpr(JsonElement source)
        : base(source)
    {
    }
}
