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

    public void Write(Program program)
    {
        ArgumentNullException.ThrowIfNull(program, nameof(program));

        _writer.Using(_namespaces);
        _writer.BeginClass(_className);
        {
            _writer.PushIndent();
            Visit(program);
            _writer.PopIndent();
        }
        _writer.EndClass();
    }

    public static string GenerateCode(Program program)
    {
        using StringWriter sw = new StringWriter();
        new CSharpProgramWriter(sw).Write(program);
        return sw.ToString();
    }
}
