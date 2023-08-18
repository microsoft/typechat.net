// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

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

    public IList<string> Namespaces => _namespaces;

    public void Write(Program program, Type apiType)
    {
        ArgumentNullException.ThrowIfNull(program, nameof(program));

        _writer.Using(_namespaces);
        _writer.BeginClass(_className);
        {
            _writer.PushIndent();
            _writer.BeginDeclareMethod("Run", CSharpLang.Types.Dynamic);
            {
                _writer.Parameter("api", apiType);
            }
            _writer.EndDeclareMethod();
            _writer.BeginMethodBody();
            {
                Visit(program);
                _writer.Return();
            }
            _writer.EndMethodBody();

            _writer.PopIndent();
        }
        _writer.EndClass();
    }

    protected override void VisitFunction(FunctionCall functionCall)
    {
        base.VisitFunction(functionCall);
    }

    public static string GenerateCode(Program program, Type apiType)
    {
        using StringWriter sw = new StringWriter();
        new CSharpProgramWriter(sw).Write(program, apiType);
        return sw.ToString();
    }
}
