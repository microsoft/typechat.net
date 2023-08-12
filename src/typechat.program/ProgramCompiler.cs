  // Copyright (c) Microsoft. All rights reserved.

using System.Linq.Expressions;
using LinqExpression = System.Linq.Expressions.Expression;

namespace Microsoft.TypeChat;


/// <summary>
/// Compiles a Json Program into a typesafe .NET lambda
/// Does so using System.Linq.Expressions and the DLR
/// </summary>
public class ProgramCompiler
{
    ApiTypeInfo _apiTypeInfo;
    ConstantExpression _apiImpl;
    Dictionary<string, ParameterExpression> _variables;

    public ProgramCompiler(Type type)
        : this(new ApiTypeInfo(type))
    {
    }

    public ProgramCompiler(ApiTypeInfo typeInfo)
    {
        ArgumentNullException.ThrowIfNull(typeInfo, nameof(typeInfo));
        _apiTypeInfo = typeInfo;
        _variables = new Dictionary<string, ParameterExpression>();
    }

    /// <summary>
    /// Return a Lambda Expression representation for the program to run against
    /// the given implementation of an api
    /// </summary>
    /// <param name="apiImpl"></param>
    /// <param name="program"></param>
    /// <returns></returns>
    public LambdaExpression CompileToExpressionTree(Program program, object apiImpl)
    {
        ArgumentNullException.ThrowIfNull(apiImpl, nameof(apiImpl));
        ArgumentNullException.ThrowIfNull(program, nameof(program));

        Clear();

        _apiImpl = LinqExpression.Constant(apiImpl);
        BlockExpression lambdaBlock = LinqExpression.Block(
            _variables.Values,
            CompileSteps(program.Steps)
        );
        return LinqExpression.Lambda(lambdaBlock);
    }

    public Delegate Compile(Program program, object apiImpl)
    {
        LambdaExpression lambda = CompileToExpressionTree(program, apiImpl);
        return lambda.Compile();
    }

    void Clear()
    {
        _apiImpl = null;
        _variables.Clear();
    }

    BlockExpression CompileSteps(Steps steps)
    {
        var block = BeginBlock();
        {
            FunctionCall[] calls = steps.Calls;
            for (int i = 0; i < calls.Length; ++i)
            {
                LinqExpression expr = CompileStep(calls[i], i);
                block.Add(expr);
            }
        }
        return EndBlock(block);
    }

    BinaryExpression CompileStep(FunctionCall call, int stepNumber)
    {
        ApiMethod method = _apiTypeInfo[call.Name];
        LinqExpression resultVar = AddVariable(method.ReturnType.ParameterType, ResultVarName(stepNumber));
        LinqExpression callExpr = Compile(call, method);
        return LinqExpression.Assign(resultVar, callExpr);
    }

    MethodCallExpression Compile(FunctionCall call)
    {
        return Compile(call, _apiTypeInfo[call.Name]);
    }

    MethodCallExpression Compile(FunctionCall call, ApiMethod method)
    {
        LinqExpression[]? args = Compile(call.Args);
        return LinqExpression.Call(_apiImpl, method.Method, args);
    }

    LinqExpression Compile(Expression expr)
    {
        switch (expr)
        {
            default:
                break;

            case FunctionCall call:
                return Compile(call);

            case ResultReference result:
                return Compile(result);

            case ValueExpr value:
                return Compile(value);

            case ArrayExpr array:
                return Compile(array);

            case ObjectExpr obj:
                return Compile(obj);
        }

        return null;
    }

    LinqExpression[]? CompileArgs(Expression[] expressions, ParameterInfo[] paramsInfo)
    {
        LinqExpression[] args = new LinqExpression[expressions.Length];
        for (int i = 0; i < expressions.Length; ++i)
        {
            switch(expressions[i])
            {
                default:
                    args[i] = Compile(expressions[i]);
                    break;

                case ObjectExpr objExpr:
                    var jsonObj = Compile(objExpr);
                    if (paramsInfo[i].ParameterType != typeof(JsonObject))
                    {
                        args[i] = DeserializeJson(jsonObj, paramsInfo[i].ParameterType);
                    }
                    else
                    {
                        args[i] = jsonObj;
                    } 
                    break;
            }
        }
        return args;
    }

    LinqExpression[]? Compile(Expression[] expressions)
    {
        if (expressions.Length == 0)
        {
            return null;
        }
        LinqExpression[] linqExpressions = new LinqExpression[expressions.Length];
        for (int i = 0; i < expressions.Length; ++i)
        {
            linqExpressions[i] = Compile(expressions[i]);
        }
        return linqExpressions;
    }

    ConstantExpression Compile(ValueExpr expr)
    {
        switch (expr.Value.ValueKind)
        {
            default:
                throw new ProgramException(ProgramException.ErrorCode.TypeNotSupported, $"{expr.Value.ValueKind}");
            case JsonValueKind.True:
                return LinqExpression.Constant(true);
            case JsonValueKind.False:
                return LinqExpression.Constant(false);
            case JsonValueKind.String:
                return LinqExpression.Constant(expr.Value.GetString());
            case JsonValueKind.Number:
                return LinqExpression.Constant(expr.Value.GetDouble());
        }
    }

    JsonNode ToJsonNode(ValueExpr expr)
    {
        switch (expr.Value.ValueKind)
        {
            default:
                throw new ProgramException(ProgramException.ErrorCode.TypeNotSupported, $"{expr.Value.ValueKind}");
            case JsonValueKind.True:
                return true;
            case JsonValueKind.False:
                return false;
            case JsonValueKind.String:
                return expr.Value.GetString();
            case JsonValueKind.Number:
                return expr.Value.GetDouble();
        }
    }

    NewArrayExpression Compile(ArrayExpr expr)
    {
        LinqExpression[] elements = Compile(expr.Value);
        return LinqExpression.NewArrayInit(typeof(object), elements);
    }

    ParameterExpression Compile(ResultReference refExpr)
    {
        return GetVariable(ResultVarName(refExpr.Ref));
    }

    BlockExpression Compile(ObjectExpr expr)
    {
        var block = BeginBlock();
        {
            MethodCallExpression addJsonPropertyExpr = null;
            JsonObject jsonObj = new JsonObject();
            var jsonObjExpr = LinqExpression.Constant(jsonObj);

            foreach (var property in expr.Value)
            {
                switch (property.Value)
                {
                    default:
                        break;

                    case ValueExpr value:
                        // Constants we can just preinject
                        JsonNode node = ToJsonNode(value);
                        jsonObj.Add(property.Key, node);
                        break;

                    case FunctionCall call:
                        // The returned JsonNode becomes a child of the json object
                        addJsonPropertyExpr = AddJsonProperty(jsonObjExpr, property.Key, CallJsonFunc(call));
                        block.Add(addJsonPropertyExpr);
                        break;

                    case ResultReference result:
                        addJsonPropertyExpr = AddJsonProperty(jsonObjExpr, property.Key, CastToJson(Compile(result)));
                        block.Add(addJsonPropertyExpr);
                        break;

                    case ObjectExpr obj:
                        addJsonPropertyExpr = AddJsonProperty(jsonObjExpr, property.Key, Compile(obj));
                        block.Add(addJsonPropertyExpr);
                        break;
                }
            }
            block.Add(jsonObjExpr);
        }
        return EndBlock(block);
    }

    ParameterExpression AddVariable(Type type, string name)
    {
        Debug.Assert(!_variables.ContainsKey(name));

        var variable = LinqExpression.Variable(type, name);
        _variables.Add(name, variable);
        return variable;
    }

    ParameterExpression? GetVariable(string name)
    {
        if (_variables.TryGetValue(name, out ParameterExpression variable))
        {
            return variable;
        }
        ProgramException.ThrowVariableNotFound(name);
        return null;
    }

    LinqExpression CallJsonFunc(FunctionCall call)
    {
        ApiMethod method = _apiTypeInfo[call.Name];
        var callExpr = Compile(call, method);
        if (method.ReturnType.ParameterType != typeof(JsonNode))
        {
            // Convert whatever the function returned to a JsonNode
            return CastToJson(callExpr);
        }
        return callExpr;
    }

    MethodCallExpression AddJsonProperty(ConstantExpression jsonObj, string name, LinqExpression value)
    {
        return LinqExpression.Call(
            CompilerApi.AddNodeMethod.Method,
            jsonObj,
            LinqExpression.Constant(name),
            value
        );
    }

    UnaryExpression CastToJson(LinqExpression expr)
    {
        return LinqExpression.Convert(expr, typeof(JsonNode));
    }

    MethodCallExpression DeserializeJson(LinqExpression jsonObj, Type type)
    {
        return LinqExpression.Call(
            CompilerApi.DeserializeMethod.Method,
            jsonObj,
            LinqExpression.Constant(type)
            );
    }

    string ResultVarName(int resultRef)
    {
        return "resultRef_" + resultRef;
    }

    List<LinqExpression> BeginBlock()
    {
        // Future: pool
        return new List<LinqExpression>();
    }
    BlockExpression EndBlock(List<LinqExpression> list)
    {
        // Pool block lists 
        return LinqExpression.Block(list);
    }

    class CompilerApi
    {
        static CompilerApi()
        {
            var typeInfo = new ApiTypeInfo(typeof(CompilerApi).GetMethods(BindingFlags.Public | BindingFlags.Static));
            AddNodeMethod = typeInfo["AddNode"];
            DeserializeMethod = typeInfo["Deserialize"];
        }

        public static readonly ApiMethod AddNodeMethod;
        public static readonly ApiMethod DeserializeMethod;

        public static object? Deserialize(JsonObject obj, Type type)
        {
            return JsonSerializer.Deserialize(obj, type);
        }

        public static JsonObject AddNode(JsonObject obj, string name, JsonNode node)
        {
            obj.Add(name, node);
            return obj;
        }
    }
}
