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
    List<LinqExpression> _block;

    public ProgramCompiler(Type type)
        : this(new ApiTypeInfo(type))
    {
    }

    public ProgramCompiler(ApiTypeInfo typeInfo)
    {
        ArgumentNullException.ThrowIfNull(typeInfo, nameof(typeInfo));
        _apiTypeInfo = typeInfo;
        _variables = new Dictionary<string, ParameterExpression>();
        _block = new List<LinqExpression>();
    }

    public System.Linq.Expressions.Expression Compile(object api, Program program)
    {
        ArgumentNullException.ThrowIfNull(api, nameof(api));
        ArgumentNullException.ThrowIfNull(program, nameof(program));

        if (!_apiTypeInfo.Type.IsAssignableFrom(api.GetType()))
        {
            throw new ArgumentException($"Api must be of type {_apiTypeInfo.Type}");
        }

        Clear();

        _apiImpl = LinqExpression.Constant(api);
        _block.Add(_apiImpl);
        _block.AddRange(CompileSteps(program.Steps));
        return LinqExpression.Block(_block);
    }

    void Clear()
    {
        _apiImpl = null;
        _variables.Clear();
        _block.Clear();
    }

    IEnumerable<LinqExpression> CompileSteps(Steps steps)
    {
        FunctionCall[] calls = steps.Calls;
        for (int i = 0; i < calls.Length; ++i)
        {
            yield return CompileStep(calls[i], i);
        }
    }

    LinqExpression CompileStep(FunctionCall call, int stepNumber)
    {
        ApiMethod method = _apiTypeInfo[call.Name];
        LinqExpression resultVar = AddVariable(method.ReturnType.ParameterType, ResultVarName(stepNumber));
        LinqExpression callExpr = Compile(call, method);
        return LinqExpression.Assign(resultVar, callExpr);
    }

    LinqExpression Compile(FunctionCall call)
    {
        return Compile(call, _apiTypeInfo[call.Name]);
    }

    LinqExpression Compile(FunctionCall call, ApiMethod method)
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

    LinqExpression Compile(ValueExpr expr)
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

    LinqExpression Compile(ArrayExpr expr)
    {
        LinqExpression[] elements = Compile(expr.Value);
        return LinqExpression.NewArrayInit(typeof(object), elements);
    }

    LinqExpression Compile(ResultReference refExpr)
    {
        return GetVariable(ResultVarName(refExpr.Ref));
    }

    LinqExpression Compile(ObjectExpr expr)
    {
        throw new NotSupportedException();
    }

    LinqExpression AddVariable(Type type, string name)
    {
        Debug.Assert(!_variables.ContainsKey(name));

        var variable = LinqExpression.Variable(type, name);
        _variables.Add(name, variable);
        return variable;
    }

    LinqExpression GetVariable(string name)
    {
        if (_variables.TryGetValue(name, out ParameterExpression variable))
        {
            return variable;
        }
        ProgramException.ThrowVariableNotFound(name);
        return null;
    }

    string ResultVarName(int resultRef)
    {
        return "resultRef_" + resultRef;
    }
}
