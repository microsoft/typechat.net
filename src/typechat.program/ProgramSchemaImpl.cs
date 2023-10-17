// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// A Json Program. See ProgramSchema.ts for the grammar for Programs
/// This class is the root is a simple AST for programs
/// </summary>
public partial class Program : IDisposable
{
    JsonDocument? _programSource;

    /// <summary>
    /// Create a new Json program from the Json document
    /// </summary>
    /// <param name="source"></param>
    /// <param name="steps"></param>
    public Program(JsonDocument? source, Steps steps)
    {
        ArgumentVerify.ThrowIfNull(steps, nameof(steps));

        _programSource = source;
        Steps = steps;
    }

    internal Program(JsonDocument? source = null)
    {
        _programSource = source;
    }

    ~Program()
    {
        Dispose(false);
    }

    [JsonIgnore]
    public JsonDocument? Source => _programSource;

    /// <summary>
    /// Did the LLM actually return steps?
    /// </summary>
    [JsonIgnore]
    public bool HasSteps => (Steps is not null && !Steps.Calls.IsNullOrEmpty());

    /// <summary>
    /// A program is deemed Complete only if it has Steps and the language model claims to have
    /// translated all of the user's request and intent into a program. 
    /// </summary>
    [JsonIgnore]
    public bool IsComplete => (HasSteps && NotTranslated.IsNullOrEmpty());

    /// <summary>
    /// Were parts of the user request not translated?
    /// </summary>
    [JsonIgnore]
    public bool HasNotTranslated => !NotTranslated.IsNullOrEmpty();

    /// <summary>
    /// Optional:
    /// A fully compiled and typesafe delegate for this program
    /// Automatically created if you use the ProgramValidator
    /// </summary>
    [JsonIgnore]
    public Delegate? Delegate { get; internal set; }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public dynamic Run(Api api)
    {
        ArgumentVerify.ThrowIfNull(api, nameof(api));
        if (!IsComplete)
        {
            throw new ProgramException(ProgramException.ErrorCode.InvalidProgramJson);
        }
        if (Delegate is not null)
        {
            return Delegate.DynamicInvoke();
        }

        ProgramInterpreter interpreter = new ProgramInterpreter();
        return interpreter.Run(this, api.Call);
    }

    public Task<dynamic> RunAsync(Api api)
    {
        ArgumentVerify.ThrowIfNull(api, nameof(api));

        if (!IsComplete)
        {
            throw new ProgramException(ProgramException.ErrorCode.InvalidProgramJson);
        }

        ProgramInterpreter interpreter = new ProgramInterpreter();
        return interpreter.RunAsync(this, api.CallAsync);
    }

    protected virtual void Dispose(bool fromDispose)
    {
        if (fromDispose)
        {
            _programSource?.Dispose();
        }
        _programSource = null;
        Delegate = null;
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
    public JsonElement Source
    {
        get;
        private set;
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
    [JsonIgnore]
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
            switch (ValueType)
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

    internal JsonNode ToJsonNode()
    {
        switch (Value.ValueKind)
        {
            default:
                throw new ProgramException(ProgramException.ErrorCode.JsonValueTypeNotSupported, $"{Value.ValueKind}");
            case JsonValueKind.True:
                return true;
            case JsonValueKind.False:
                return false;
            case JsonValueKind.String:
                return Value.GetString();
            case JsonValueKind.Number:
                return Value.GetDouble();
        }
    }

}

public partial class ArrayExpr : Expression
{
    public ArrayExpr(JsonElement source, Expression[] exprs)
        : base(source)
    {
        ArgumentVerify.ThrowIfNull(exprs, nameof(exprs));
        Value = exprs;
    }

    internal override Type Type => typeof(object[]);
}

public partial class ObjectExpr : Expression
{
    public ObjectExpr(JsonElement source, Dictionary<string, Expression> obj)
        : base(source)
    {
        ArgumentVerify.ThrowIfNull(obj, nameof(obj));
        Value = obj;
    }

    internal override Type Type => typeof(JsonObject);
}

public partial class NotTranslatedExpr : Expression
{
    public NotTranslatedExpr(JsonElement source, string text)
        : base(source)
    {
        Text = text;
    }
}

public partial class UnknownExpr : Expression
{
    public UnknownExpr(JsonElement source)
        : base(source)
    {
    }
}
