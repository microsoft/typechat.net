// Copyright (c) Microsoft. All rights reserved.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Microsoft.TypeChat;

public class CSharpProgramCompiler
{
    string _assemblyName;

    public CSharpProgramCompiler(string assemblyName)
    {
        _assemblyName = assemblyName;
    }

    public string GetDiagnostics(string code, string apiFilePath)
    {
        var apiTree = CSharpSyntaxTree.ParseText(apiFilePath);
        var codeTree = CSharpSyntaxTree.ParseText(code);
        var compilation = CSharpCompilation.Create(_assemblyName).WithOptions(
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );
        compilation.AddSyntaxTrees(apiTree, codeTree);

        var diagnostics = compilation.GetDiagnostics();
        StringBuilder sb = new StringBuilder();
        foreach (var diagnostic in diagnostics)
        {
            sb.AppendLine($"{diagnostic.Id}: {diagnostic.GetMessage()}");
            sb.AppendLine($"Location: {diagnostic.Location.GetLineSpan()}");
            sb.AppendLine($"Severity: {diagnostic.Severity}");
            sb.AppendLine();
        }
        return sb.ToString();
    }
}
