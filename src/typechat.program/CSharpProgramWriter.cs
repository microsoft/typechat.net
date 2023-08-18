// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// Transpile Json Programs into C#
/// ClassName: Program (Default)
/// MethodName: Run
/// </summary>
public class CSharpProgramWriter : ProgramVisitor
{
    const string DefaultClassName = "Program";
    static string[] s_standardNamespaces = new[] { "System", "System.Text" };

    CSharpWriter _writer;
    List<string> _namespaces;
    string _className;

    public CSharpProgramWriter(TextWriter writer)
    {
        _writer = new CSharpWriter(writer);
        _className = DefaultClassName;
        _namespaces = new List<string>();
        _namespaces.AddRange(s_standardNamespaces);
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

    public void Write(Program program, Type apiType)
    {
        ArgumentNullException.ThrowIfNull(program, nameof(program));

        _writer.Using(_namespaces);
        _writer.Using(apiType.Namespace);
        _writer.BeginClass(_className);
        {
            WriteMethod(program, apiType);
        }
        _writer.EndClass();
    }

    void WriteMethod(Program program, Type apiType)
    {
        _writer.BeginDeclareMethod(MethodName, CSharpLang.Types.Dynamic);
        {
            _writer.Variable(ApiVarName, apiType);
        }
        _writer.EndDeclareMethod();
        _writer.BeginMethodBody();
        {
            Visit(program);
        }
        _writer.EndMethodBody();
    }

    protected override void VisitSteps(Steps steps)
    {
        base.VisitSteps(steps);
        if (!steps.Calls.IsNullOrEmpty())
        {
            _writer.Return(ResultVar(steps.Calls.Length - 1));
        }
    }

    protected override void VisitStep(FunctionCall function, int stepNumber)
    {
        _writer.SOL();
        _writer.Local(ResultVar(stepNumber), CSharpLang.Types.Var, isArray: false, assign: true);
        VisitFunction(function, false);
    }

    protected override void VisitFunction(FunctionCall function)
    {
        VisitFunction(function, true);
    }

    void VisitFunction(FunctionCall function, bool inline)
    {
        _writer.BeginCall(ApiVarName, function.Name);
        {
            VisitArgs(function);
        }
        _writer.EndCall(inline);
    }

    void VisitArgs(FunctionCall function)
    {
        var args = function.Args;
        if (args.IsNullOrEmpty())
        {
            return;
        }
        for (int i = 0; i < args.Length; ++i)
        {
            if (i > 0) { _writer.ArgSep(); }
            Visit(args[i]);
        }
    }

    protected override void VisitValue(ValueExpr valueExpr)
    {
        _writer.Append(valueExpr.Value.Stringify());
    }

    protected override void VisitResult(ResultReference resultRef)
    {
        _writer.Append(ResultVar(resultRef.Ref));
    }

    string ResultVar(int resultNumber) => (ResultVarPrefix + (resultNumber + 1));


    public static string GenerateCode(Program program, Type apiType)
    {
        using StringWriter sw = new StringWriter();
        new CSharpProgramWriter(sw).Write(program, apiType);
        return sw.ToString();
    }
}

