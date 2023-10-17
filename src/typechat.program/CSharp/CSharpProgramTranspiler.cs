﻿// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.CSharp;

/// <summary>
/// Experimental
/// Class that transpiles Json Programs into C#.
/// Programs are emitted into a class:
///     ClassName: Program
///     MethodName: Run
///
/// Only SYNCHRONOUS methods currently supported
/// Async will be added in a future release
/// 
/// </summary>
public class CSharpProgramTranspiler
{
    const string DefaultClassName = "Program";
    static string[] s_standardNamespaces = new[] { "System", "System.Text", "System.Text.Json", "System.Text.Json.Nodes" };

    Type _apiType;
    ApiTypeInfo _apiTypeInfo;
    List<string> _namespaces;
    string _className;
    List<string> _blocks;
    int _objectId = 0;
    int _minIndent = 0;

    public CSharpProgramTranspiler(Type type, ApiTypeInfo? typeInfo = null)
    {
        ArgumentVerify.ThrowIfNull(type, nameof(type));
        typeInfo ??= new ApiTypeInfo(type);
        //
        // Currently we don't support async operations
        //
        if (typeInfo.HasAsyncMethods())
        {
            throw new NotSupportedException("Async methods currently not supported");
        }
        _apiType = type;
        _apiTypeInfo = typeInfo;
        _className = DefaultClassName;
        _namespaces = new List<string>();
        _namespaces.AddRange(s_standardNamespaces);
        _blocks = new List<string>();
    }

    /// <summary>
    /// Your program is transpiled into a class with this name
    /// </summary>
    public string ClassName
    {
        get => _className;
        set
        {
            ArgumentVerify.ThrowIfNullOrEmpty(value, nameof(ClassName));
            _className = value;
        }
    }

    internal string ApiVarName { get; set; } = "api";
    internal string ResultVarPrefix { get; set; } = "step";
    internal string MethodName { get; set; } = "Run";

    public IList<string> Namespaces => _namespaces;

    /// <summary>
    /// Transpiles the program to C#
    /// </summary>
    /// <param name="program"></param>
    /// <returns>Program source code</returns>
    public string Compile(Program program)
    {
        ArgumentVerify.ThrowIfNull(program, nameof(program));

        var (buffer, writer) = BeginBlock();
        {
            _minIndent = 1;
            writer.Using(_namespaces);
            writer.Using(_apiType.Namespace);
            writer.BeginClass(_className);
            {
                writer.Append(CompileProgramMethods(program));
                //
                // Emit all blocks generated during compilation
                //
                writer.EOL();
                foreach (string block in _blocks)
                {
                    writer.Append(block).EOL();
                }
            }
            writer.EndClass();
        }
        return EndBlock(buffer, writer, false);
    }

    void Clear()
    {
        _objectId = 0;
        _blocks.Clear();
    }

    string CompileProgramMethods(Program program)
    {
        var (buffer, writer) = BeginBlock();
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
        return EndBlock(buffer, writer, false);
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

    string Compile(FunctionCall function, bool inline = true)
    {
        var (sw, writer) = BeginBlock();
        {
            Compile(writer, function, inline);
        }
        return EndBlock(sw, writer, false);
    }

    void Compile(CSharpWriter writer, FunctionCall function, bool inline)
    {
        var apiInfo = _apiTypeInfo[function.Name];
        writer.BeginMethodCall(ApiVarName, function.Name);
        {
            CompileArgs(writer, function, apiInfo.Params);
        }
        writer.EndMethodCall(inline);
    }

    void CompileArgs(CSharpWriter writer, FunctionCall function, ParameterInfo[] paramsInfo)
    {
        Expression[] expressions = function.Args;
        if (expressions.IsNullOrEmpty())
        {
            return;
        }
        if (paramsInfo.Length != expressions.Length)
        {
            ProgramException.ThrowArgCountMismatch(function, paramsInfo.Length, expressions.Length);
        }

        for (int i = 0; i < expressions.Length; ++i)
        {
            ParameterInfo param = paramsInfo[i];

            if (i > 0) { writer.ArgSep(); }
            switch (expressions[i])
            {
                default:
                    writer.Append(Compile(expressions[i]));
                    break;

                case ArrayExpr arrayExpr:
                    writer.Append(
                        Compile(arrayExpr, param.ParameterType.IsArray ?
                                param.ParameterType.GetElementType().Name :
                                null)
                    );
                    break;

                case ObjectExpr objExpr:
                    var jsonObjExpr = Compile(objExpr);
                    if (!param.CanBeDeserialized())
                    {
                        // Can't deserialize an object to a primitive type
                        ProgramException.ThrowTypeMismatch(function, param, objExpr.Type);
                    }
                    CastFromJsonObject(writer, jsonObjExpr, paramsInfo[i].ParameterType);
                    break;
            }
        }
    }

    string Compile(Expression expr)
    {
        switch (expr)
        {
            default:
                throw new NotSupportedException();

            case FunctionCall call:
                return Compile(call);

            case ResultReference result:
                return Compile(result);

            case ValueExpr value:
                return Compile(value);

            case ArrayExpr array:
                return Compile(array, CSharpLang.Types.Dynamic);

            case ObjectExpr obj:
                return Compile(obj);
        }
    }

    string Compile(ValueExpr expr)
    {
        switch (expr.Value.ValueKind)
        {
            default:
                throw new ProgramException(ProgramException.ErrorCode.JsonValueTypeNotSupported, $"{expr.Value.ValueKind}");
            case JsonValueKind.True:
                return CSharpLang.Types.True;
            case JsonValueKind.False:
                return CSharpLang.Types.False;
            case JsonValueKind.String:
                return CSharpLang.String(expr.Value.GetString());
            case JsonValueKind.Number:
                return CSharpLang.Double(expr.Value.GetDouble());
        }
    }

    string Compile(ResultReference resultRef)
    {
        return ResultVar(resultRef.Ref);
    }

    string Compile(ArrayExpr arrayExpr, string type)
    {
        var (sw, writer) = BeginBlock();
        {
            Compile(writer, arrayExpr.Value, type);
        }
        return EndBlock(sw, writer, false);
    }

    string Compile(Expression[] expressions, string type = null)
    {
        var (sw, writer) = BeginBlock();
        {
            Compile(writer, expressions, type);
        }
        return EndBlock(sw, writer, false);
    }

    void Compile(CSharpWriter writer, Expression[] expressions, string type)
    {
        writer.BeginArray(type);
        {
            for (int i = 0; i < expressions.Length; ++i)
            {
                if (i > 0) { writer.ArgSep(); }
                writer.Append(Compile(expressions[i]));
            }
        }
        writer.EndArray();
    }

    /// <summary>
    /// Object Expressions are compiled into a factory method
    /// </summary>
    /// <param name="objectExpr"></param>
    /// <returns>A call to the factory method</returns>
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
                writer.SOL();
                writer.Local(jsonObj, nameof(JsonObject), false, true).New(nameof(JsonObject)).Semicolon().EOL();
                foreach (var property in objectExpr.Value)
                {
                    switch (property.Value)
                    {
                        default:
                            break;

                        case FunctionCall call:
                            AddJsonProperty(
                                writer,
                                jsonObj,
                                property.Key,
                                Compile(call),
                                _apiTypeInfo[call.Name].ReturnType.ParameterType
                            );
                            break;

                        case ValueExpr value:
                            AddJsonProperty(writer, jsonObj, property.Key, Compile(value), value.Type);
                            break;

                        case ArrayExpr array:
                            AddJsonProperty(writer, jsonObj, property.Key, Compile(array), typeof(Array));
                            break;

                        case ObjectExpr obj:
                            AddJsonProperty(writer, jsonObj, property.Key, Compile(obj));
                            break;
                    }
                }
                writer.Return(jsonObj);
            }
            writer.EndMethodBody();
        }
        EndBlock(buffer, writer);
        //
        // Emit a call to the method
        //
        return methodName + "()";
    }

    void AddJsonProperty(CSharpWriter writer, string jsonObj, string key, string value, Type? valueType = null)
    {
        writer.SOL();
        writer.BeginMethodCall(jsonObj, "Add");
        {
            writer.Literal(key).ArgSep();
            if (valueType is not null)
            {
                if (valueType.IsString() ||
                    valueType.IsPrimitive)
                {
                    writer.Append(value);
                }
                else
                {
                    //
                    // Direct cast not available. Serialize to JsonNode first
                    //
                    writer.StaticCall("JsonSerializer.SerializeToNode", value, CSharpLang.TypeOf(valueType.Name));
                }
            }
            else
            {
                writer.Append(value);
            }
        }
        writer.EndMethodCall();
    }

    void CastFromJsonObject(CSharpWriter writer, string jsonObj, Type type)
    {
        if (!typeof(JsonObject).IsAssignableFrom(type))
        {
            string targetType = type.Name;
            writer.Cast(targetType);
            writer.StaticCall("JsonSerializer.Deserialize", jsonObj, CSharpLang.TypeOf(targetType));
        }
    }

    string ResultVar(int resultNumber) => ResultVarPrefix + (resultNumber + 1);

    (StringWriter, CSharpWriter) BeginBlock()
    {
        var sw = new StringWriter();
        var writer = new CSharpWriter(sw);
        for (int i = 0; i < _minIndent; ++i)
        {
            writer.PushIndent();
        }
        return (sw, writer);
    }
    string EndBlock(StringWriter sw, CSharpWriter writer, bool emit = true)
    {
        string codeBlock = sw.ToString();
        if (emit)
        {
            _blocks.Add(codeBlock);
        }
        return codeBlock;
    }

    public static string GenerateCode(Program program, Type apiType)
    {
        return new CSharpProgramTranspiler(apiType).Compile(program);
    }
}

