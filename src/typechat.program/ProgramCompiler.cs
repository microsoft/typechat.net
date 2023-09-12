// Copyright (c) Microsoft. All rights reserved.

using System.Linq.Expressions;
using LinqExpression = System.Linq.Expressions.Expression;

namespace Microsoft.TypeChat.Schema;

/// <summary>
/// Compiles a Json Program into a typesafe .NET lambda using the Dynamic Language Runtime
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

    public Delegate Compile(Program program, Api api)
    {
        return Compile(program, api.Implementation);
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

    LinqExpression CompileStep(FunctionCall call, int stepNumber)
    {
        ApiMethod method = _apiTypeInfo[call.Name];
        LinqExpression callExpr = Compile(call, method);
        if (callExpr.Type != typeof(void))
        {
            LinqExpression resultVar = AddVariable(callExpr.Type, ResultVarName(stepNumber));
            return LinqExpression.Assign(resultVar, callExpr);
        }
        return callExpr;
    }

    LinqExpression Compile(FunctionCall call)
    {
        return Compile(call, _apiTypeInfo[call.Name]);
    }

    LinqExpression Compile(FunctionCall call, ApiMethod method)
    {
        LinqExpression[]? args = CompileArgs(call, method.Params);
        MethodCallExpression callExpr = LinqExpression.Call(_apiImpl, method.Method, args);
        return CompileReturnValue(callExpr);
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

    LinqExpression[]? CompileArgs(FunctionCall call, ParameterInfo[] paramsInfo)
    {
        Expression[] expressions = call.Args;
        if (paramsInfo.Length != expressions.Length)
        {
            ProgramException.ThrowArgCountMismatch(call, paramsInfo.Length, expressions.Length);
        }

        LinqExpression[] args = new LinqExpression[expressions.Length];
        for (int i = 0; i < expressions.Length; ++i)
        {
            ParameterInfo param = paramsInfo[i];
            switch (expressions[i])
            {
                default:
                    args[i] = Compile(expressions[i]);
                    break;

                case ValueExpr valueExpr:
                    Type valueType = valueExpr.Type;
                    if (!param.IsConvertibleFrom(valueType))
                    {
                        ProgramException.ThrowTypeMismatch(call, param, valueExpr.Type);
                    }
                    LinqExpression value = Compile(valueExpr);
                    if (param.ParameterType != valueExpr.Type)
                    {
                        value = LinqExpression.Convert(value, param.ParameterType);
                    }
                    args[i] = value;
                    break;

                case ArrayExpr arrayExpr:
                    args[i] = Compile(
                        arrayExpr,
                        param.ParameterType.IsArray ?
                        param.ParameterType.GetElementType() :
                        null);
                    break;

                case ObjectExpr objExpr:
                    Type objType = objExpr.Type;
                    if (!param.CanBeDeserialized())
                    {
                        // Can't deserialize an object to a primitive type
                        ProgramException.ThrowTypeMismatch(call, param, objType);
                    }
                    var jsonObjExpr = Compile(objExpr);
                    args[i] = CastFromJsonObject(jsonObjExpr, param.ParameterType);
                    break;
            }
        }
        return args;
    }

    LinqExpression[]? Compile(Expression[] expressions, Type? itemType = null)
    {
        if (expressions.Length == 0)
        {
            return null;
        }
        LinqExpression[] items = new LinqExpression[expressions.Length];
        for (int i = 0; i < expressions.Length; ++i)
        {
            items[i] = Compile(expressions[i]);
            if (itemType != null)
            {
                items[i] = CastFromJsonObject(items[i], itemType);
            }
        }
        return items;
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

    NewArrayExpression Compile(ArrayExpr expr, Type? itemType = null)
    {
        LinqExpression[] items = Compile(expr.Value, itemType);
        itemType ??= typeof(object);
        return LinqExpression.NewArrayInit(itemType, items);
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
                        throw new InvalidOperationException();

                    case ValueExpr value:
                        // Constants we can just preinject
                        JsonNode node = value.ToJsonNode();
                        jsonObj.Add(property.Key, node);
                        break;

                    case FunctionCall call:
                        // The returned JsonNode becomes a child of the json object
                        addJsonPropertyExpr = AddJsonProperty(jsonObjExpr, property.Key, CallJsonFunc(call));
                        block.Add(addJsonPropertyExpr);
                        break;

                    case ResultReference result:
                        var resultExpr = Compile(result);
                        addJsonPropertyExpr = AddJsonProperty(jsonObjExpr, property.Key, CastToJsonNode(resultExpr, resultExpr.Type));
                        block.Add(addJsonPropertyExpr);
                        break;

                    case ArrayExpr arrayExpr:
                        addJsonPropertyExpr = AddJsonProperty(jsonObjExpr, property.Key, Compile(arrayExpr));
                        block.Add(addJsonPropertyExpr);
                        break;

                    case ObjectExpr obj:
                        addJsonPropertyExpr = AddJsonProperty(jsonObjExpr, property.Key, Compile(obj));
                        block.Add(addJsonPropertyExpr);
                        break;

                    case UnknownExpr unknown:
                        throw new ProgramException(ProgramException.ErrorCode.UnknownExpression, unknown.Source.Stringify());
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

    UnaryExpression CallJsonFunc(FunctionCall call)
    {
        ApiMethod method = _apiTypeInfo[call.Name];
        var callExpr = Compile(call, method);
        // Convert whatever the function returned to a JsonNode
        return CastToJsonNode(callExpr, method.ReturnType.ParameterType);
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

    UnaryExpression CastToJsonNode(LinqExpression srcExpr, Type srcType)
    {
        if (!(srcType.IsValueType || srcType.IsString()))
        {
            //
            // Direct cast not available. Serialize to JsonNode
            //
            srcExpr = LinqExpression.Call(CompilerApi.SerializeMethod.Method, srcExpr);
        }
        return LinqExpression.Convert(srcExpr, typeof(JsonNode));
    }

    LinqExpression CastFromJsonObject(LinqExpression srcExpr, Type type)
    {
        if (srcExpr.Type == type)
        {
            return srcExpr;
        }
        if (!type.IsJsonObject())
        {
            srcExpr = LinqExpression.Call(
                CompilerApi.DeserializeMethod.Method,
                srcExpr,
                LinqExpression.Constant(type)
                );
        }
        return LinqExpression.Convert(srcExpr, type);
    }

    LinqExpression CompileReturnValue(LinqExpression retVal)
    {
        Type retType = retVal.Type;

        if (retType.IsAssignableTo(typeof(Task)))
        {
            if (retType.IsGenericType)
            {
                Debug.Assert(!retType.GenericTypeArguments.IsNullOrEmpty());
                LinqExpression waitResult = LinqExpression.Call(CompilerApi.WaitForResultMethod.Method, retVal);
                return LinqExpression.Convert(waitResult, retType.GenericTypeArguments[0]);
            }

            return LinqExpression.Call(CompilerApi.WaitForMethod.Method, retVal);
        }

        return retVal;
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
            SerializeMethod = typeInfo["Serialize"];
            WaitForResultMethod = typeInfo["WaitForResult"];
            WaitForMethod = typeInfo["WaitFor"];
        }

        public static readonly ApiMethod AddNodeMethod;
        public static readonly ApiMethod DeserializeMethod;
        public static readonly ApiMethod SerializeMethod;
        public static readonly ApiMethod WaitForResultMethod;
        public static readonly ApiMethod WaitForMethod;

        /*
         * These methods are called dynamically through code gen
         * Do NOT delete or alter them without testing
         */

        public static object? Deserialize(JsonObject obj, Type type)
        {
            return JsonSerializer.Deserialize(obj, type);
        }

        public static JsonObject Serialize(object obj)
        {
            return (JsonObject)JsonSerializer.Serialize(obj);
        }

        public static JsonObject AddNode(JsonObject obj, string name, JsonNode node)
        {
            obj.Add(name, node);
            return obj;
        }

        public static void WaitFor(Task task)
        {
            task.ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public static dynamic WaitForResult(Task task)
        {
            dynamic dtask = task;
            return dtask.ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
