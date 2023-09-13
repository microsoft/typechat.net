// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.TypeChat;

/// <summary>
/// Compile Json Programs that were first transpiled into C#
/// 
/// </summary>
public class CSharpProgramCompiler
{
    public const string DefaultProgramName = "Program";

    public static CSharpCompilationOptions DefaultOptions()
    {
        return new CSharpCompilationOptions(
            OutputKind.DynamicallyLinkedLibrary,
            optimizationLevel: OptimizationLevel.Debug
            );
    }

    public static string GetRuntimeLocation()
    {
        string sysLocation = typeof(object).Assembly.Location;
        return Path.GetDirectoryName(sysLocation);
    }

    static CSharpCompilationOptions s_defaultOptions = DefaultOptions();

    string _assemblyName;
    CSharpCompilation _compilation;

    public CSharpProgramCompiler(string programName = DefaultProgramName)
    {
        ArgumentException.ThrowIfNullOrEmpty(programName, nameof(programName));
        _assemblyName = programName;
        _compilation = CSharpCompilation.Create(_assemblyName).WithOptions(s_defaultOptions);
    }

    public void AddCode(string code)
    {
        var apiTree = CSharpSyntaxTree.ParseText(code);
        _compilation = _compilation.AddSyntaxTrees(apiTree);
    }

    public void AddCodeFile(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        _compilation = _compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(SourceText.From(stream)));
    }

    public void AddReferences(AssemblyReferences list)
    {
        var references = from path in list
                         select MetadataReference.CreateFromFile(path);
        _compilation = _compilation.AddReferences(references);
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

    public Result<byte[]> Compile(string? code = null)
    {
        if (!string.IsNullOrEmpty(code))
        {
            AddCode(code);
        }
        using MemoryStream stream = new MemoryStream();
        var result = _compilation.Emit(stream);
        string? message = CollectDiagnostics(result.Diagnostics);
        if (result.Success)
        {
            return new Result<byte[]>(stream.ToArray(), message);
        }

        return Result<byte[]>.Error(CollectDiagnostics(result.Diagnostics));
    }

    string CollectDiagnostics(ImmutableArray<Diagnostic> diagnostics)
    {
        if (diagnostics == null || diagnostics.Length == 0)
        {
            return null;
        }
        StringBuilder sb = new StringBuilder();
        foreach (var diagnostic in diagnostics)
        {
            sb.AppendLine($"{diagnostic.Id}: {diagnostic.GetMessage()}");
            if (diagnostic.Location != Location.None)
            {
                sb.AppendLine($"Location: {diagnostic.Location.GetLineSpan()}");
                sb.AppendLine(GetTextWithError(diagnostic));
            }
        }
        return sb.ToString();
    }

    string GetTextWithError(Diagnostic diagnostic)
    {
        var errorSpan = diagnostic.Location.SourceSpan;
        var sourceText = diagnostic.Location.SourceTree.GetText();
        var errorText = sourceText.GetSubText(errorSpan.Start).ToString();
        return errorText;
    }

    /// <summary>
    /// Compile the Json program into a .NET aseembly
    /// </summary>
    /// <param name="program"></param>
    /// <returns>In in-memory compiled assembly</returns>
    public static Result<ProgramAssembly> Compile(Program program, Type apiType)
    {
        var transpiler = new CSharpProgramTranspiler(apiType);
        string code = transpiler.Compile(program);

        CSharpProgramCompiler compiler = new CSharpProgramCompiler();
        AssemblyReferences refs = new AssemblyReferences();
        refs.AddStandard();
        refs.Add(apiType);
        compiler.AddReferences(refs);

        Result<byte[]> result = compiler.Compile(code);
        if (result.Success)
        {
            ProgramAssembly assembly = new ProgramAssembly(result.Value, transpiler.ClassName, transpiler.MethodName);
            return new Result<ProgramAssembly>(assembly);
        }

        return Result<ProgramAssembly>.Error(result.Message);
    }
}

public class AssemblyReferences : HashSet<string>
{
    public AssemblyReferences() { }

    public void AddStandard()
    {
        Add(
            typeof(object),
            typeof(System.Runtime.CompilerServices.DynamicAttribute)
        );
        string runtimeDir = CSharpProgramCompiler.GetRuntimeLocation();
        Add(runtimeDir, "System.Runtime.dll");
        Add(runtimeDir, "System.Collections.dll");
        Add(runtimeDir, "System.Text.Json.dll");
        Add(runtimeDir, "System.Threading.dll");
        Add(runtimeDir, "System.Threading.Tasks.dll");
    }

    public void Add(string rootPath, string assemblyName)
    {
        string assemblyPath = Path.Join(rootPath, assemblyName);
        if (!File.Exists(assemblyPath))
        {
            throw new ArgumentException($"{assemblyPath} not found");
        }
        Add(assemblyPath);
    }

    public void Add(params Type[] types)
    {
        if (types.IsNullOrEmpty())
        {
            return;
        }
        for (int i = 0; i < types.Length; ++i)
        {
            string path = types[i].Assembly.Location;
            if (!Contains(path))
            {
                Add(path);
            }
        }
    }
}


