// Copyright (c) Microsoft. All rights reserved.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.TypeChat;

public class CSharpProgramCompiler
{
    public class ReferencesList : List<MetadataReference>
    {
        public ReferencesList() { }

        public void Add(params Type[] types)
        {
            if (types != null)
            {
                for (int i = 0; i < types.Length; ++i)
                {
                    Add(types[i].Assembly.Location);
                }
            }
        }

        public void Add(string filePath)
        {
            var reference = MetadataReference.CreateFromFile(filePath);
            Add(reference);
        }
    }

    public const string DefaultProgramName = "Program";

    public static CSharpCompilationOptions DefaultOptions()
    {
        return new CSharpCompilationOptions(
            OutputKind.DynamicallyLinkedLibrary,
            optimizationLevel: OptimizationLevel.Release
            );
    }

    static CSharpCompilationOptions s_defaultOptions = DefaultOptions();

    string _programName;
    string _assemblyName;
    CSharpCompilation _compilation;
    ReferencesList _refs;

    public CSharpProgramCompiler(string programName = DefaultProgramName)
    {
        ArgumentException.ThrowIfNullOrEmpty(programName, nameof(programName));
        _programName = programName;
        _compilation = CSharpCompilation.Create(_programName).WithOptions(s_defaultOptions);
    }

    public void AddCode(string code)
    {
        var apiTree = CSharpSyntaxTree.ParseText(code);
        _compilation.AddSyntaxTrees(apiTree);
    }

    public void AddCodeFile(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        _compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(SourceText.From(stream)));
    }

    public void AddReferences(ReferencesList list)
    {
        _compilation.AddReferences(list);
    }

    public void AddReferences(params Type[] types)
    {
        if (types != null)
        {
            ReferencesList refs = new ReferencesList();
            refs.Add(types);
            AddReferences(refs);
        }
    }

    public string? GetDiagnostics()
    {
        var diagnostics = _compilation.GetDiagnostics();
        if (diagnostics.Length > 0)
        {
            return CollectDiagnostics(diagnostics);
        }
        return null;
    }

    public Result<MemoryStream> Compile(string? code = null)
    {
        if (!string.IsNullOrEmpty(code))
        {
            AddCode(code);
        }
        MemoryStream stream = new MemoryStream();
        var result = _compilation.Emit(stream);
        if (result.Success)
        {
            return stream;
        }
        return Result<MemoryStream>.Error(CollectDiagnostics(result.Diagnostics));
    }

    string CollectDiagnostics(IEnumerable<Diagnostic> diagnostics)
    {
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
