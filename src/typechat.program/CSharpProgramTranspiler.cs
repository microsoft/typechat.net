// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// Transpile Json Programs into C#
/// ClassName: Program (Default)
/// MethodName: Run
///
/// Only synchronous implemented. Coming: async, await. 
/// </summary>
public class CSharpProgramTranspiler
{
    const string DefaultClassName = "Program";
    static string[] s_standardNamespaces = new[] { "System", "System.Text" };

    Type _apiType;
    ApiTypeInfo _apiTypeInfo;
    List<string> _namespaces;
    string _className;
    List<string> _blocks;
    int _objectId = 0;

    public CSharpProgramTranspiler(Type type, ApiTypeInfo? typeInfo = null)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));
        _apiType = type;
        typeInfo ??= new ApiTypeInfo(type);
        _apiTypeInfo = typeInfo;
        _className = DefaultClassName;
        _namespaces = new List<string>();
        _namespaces.AddRange(s_standardNamespaces);
        _blocks = new List<string>();
    }

    public string ClassName
    {
        get => _className;
        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value, nameof(ClassName));
            _className = value;
        }
    }
    public string ApiVarName { get; set; } = "api";
    public string ResultVarPrefix { get; set; } = "step";
    public string MethodName { get; set; } = "Run";

    public IList<string> Namespaces => _namespaces;

    public string Compile(Program program)
    {
        ArgumentNullException.ThrowIfNull(program, nameof(program));

        var (buffer, writer) = BeginBlock();
        {
            writer.Using(_namespaces);
            writer.Using(_apiType.Namespace);
            writer.BeginClass(_className);
            {
                Compile(writer, program);
            }
            writer.EndClass();
        }
        EndBlock(buffer, writer);
        //
        // Combine all blocks
        //
        return string.Join(Environment.NewLine, _blocks);
    }

    void Clear()
    {
        _objectId = 0;
        _blocks.Clear();
    }

    void Compile(CSharpWriter writer, Program program)
    {
        writer.BeginDeclareMethod(MethodName, CSharpLang.Types.Dynamic);
        {
            writer.Variable(ApiVarName, _apiType);
        }
        writer.EndDeclareMethod();
        writer.BeginMethodBody();
        {
            Compile(writer, program.Steps);
        }
        writer.EndMethodBody();
    }

    void Compile(CSharpWriter writer, Steps steps)
    {
        FunctionCall[] calls = steps.Calls;
        for (int i = 0; i < calls.Length; ++i)
        {
            writer.SOL();
            writer.Local(ResultVar(i), CSharpLang.Types.Var, isArray: false, assign: true);
            Compile(writer, calls[i], false);
        }
        if (!steps.Calls.IsNullOrEmpty())
        {
            writer.Return(ResultVar(steps.Calls.Length - 1));
        }
    }

    void Compile(CSharpWriter writer, FunctionCall function, bool inline)
    {
        writer.BeginMethodCall(ApiVarName, function.Name);
        {
            Compile(writer, function.Args);
        }
        writer.EndMethodCall(inline);
    }

    void Compile(CSharpWriter writer, Expression[] expressions)
    {
        if (expressions.IsNullOrEmpty())
        {
            return;
        }
        for (int i = 0; i < expressions.Length; ++i)
        {
            if (i > 0) { writer.ArgSep(); }
            Compile(writer, expressions[i]);
        }
    }

    void Compile(CSharpWriter writer, Expression expr)
    {
        switch (expr)
        {
            default:
                throw new NotSupportedException();

            case FunctionCall call:
                Compile(writer, call, true);
                break;

            case ResultReference result:
                writer.Append(Compile(result));
                break;

            case ValueExpr value:
                writer.Append(Compile(value));
                break;
        }
    }

    string Compile(ValueExpr expr)
    {
        switch (expr.Value.ValueKind)
        {
            default:
                throw new ProgramException(ProgramException.ErrorCode.TypeNotSupported, $"{expr.Value.ValueKind}");
            case JsonValueKind.True:
                return CSharpLang.Types.True;
            case JsonValueKind.False:
                return CSharpLang.Types.False;
            case JsonValueKind.String:
                return expr.Value.GetString();
            case JsonValueKind.Number:
                return expr.Value.GetDouble().ToString();
        }
    }

    string Compile(ResultReference resultRef)
    {
        return ResultVar(resultRef.Ref);
    }

    /// <summary>
    /// Object Expressions are compiled into a factory method
    /// </summary>
    /// <param name="objectExpr"></param>
    /// <returns>The name of the factory method</returns>
    string Compile(ObjectExpr objectExpr)
    {
        ++_objectId;
        string jsonObj = "jsonObj";
        string methodName = "NewJsonObject_" + _objectId;

        var (buffer, writer) = BeginBlock();
        {
            writer.DeclareMethod(methodName, nameof(JsonObject));
            writer.BeginMethodBody();
            {
                foreach (var property in objectExpr.Value)
                {
                    switch (property.Value)
                    {
                        default:
                            break;

                        case ValueExpr value:
                            writer.MethodCall(jsonObj, "Add", Compile(value));
                            break;
                    }
                }
            }
            writer.EndMethodBody();
        }
        return EndBlock(buffer, writer);
    }

    string ResultVar(int resultNumber) => (ResultVarPrefix + (resultNumber + 1));

    (StringWriter, CSharpWriter) BeginBlock()
    {
        var sw = new StringWriter();
        var writer = new CSharpWriter(sw);
        return (sw, writer);
    }
    string EndBlock(StringWriter sw, CSharpWriter writer)
    {
        string codeBlock = sw.ToString();
        _blocks.Add(codeBlock);
        return codeBlock;
    }

    public static string GenerateCode(Program program, Type apiType)
    {
        return new CSharpProgramTranspiler(apiType).Compile(program);
    }
}

