// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.TypeChat.CSharp;

/// <summary>
/// Experimental
/// CSharp compiler. Uses Roslyn to compile C# code to assemblies
/// While the compiler is generic, it is intended to be used in tandem with CSharpProgramTranspiler
/// </summary>
public class CSharpProgramCompiler
{
    public const string DefaultProgramName = "Program";

    /// <summary>
    /// Default compiler options - compiles the program as a DLL
    /// Optimizations are disabled
    /// </summary>
    /// <returns></returns>
    public static CSharpCompilationOptions DefaultOptions()
    {
        return new CSharpCompilationOptions(
            OutputKind.DynamicallyLinkedLibrary,
            optimizationLevel: OptimizationLevel.Debug
            );
    }

    /// <summary>
    /// Return the location of the runtime this assembly is bound to
    /// </summary>
    /// <returns></returns>
    public static string GetRuntimeLocation()
    {
        string sysLocation = typeof(object).Assembly.Location;
        return Path.GetDirectoryName(sysLocation);
    }

    static CSharpCompilationOptions s_defaultOptions = DefaultOptions();

    string _assemblyName;
    CSharpCompilation _compilation;

    /// <summary>
    /// Create a new compiler
    /// </summary>
    /// <param name="programName">Name of the assembly</param>
    public CSharpProgramCompiler(string programName = DefaultProgramName)
    {
        ArgumentVerify.ThrowIfNullOrEmpty(programName, nameof(programName));
        _assemblyName = programName;
        _compilation = CSharpCompilation.Create(_assemblyName).WithOptions(s_defaultOptions);
    }

    /// <summary>
    /// Add code as a new compilation unit
    /// </summary>
    /// <param name="code"></param>
    public void AddCode(string code)
    {
        var apiTree = CSharpSyntaxTree.ParseText(code);
        _compilation = _compilation.AddSyntaxTrees(apiTree);
    }
    /// <summary>
    /// Load source code from a file as a compilation unit
    /// </summary>
    /// <param name="filePath">file path</param>
    public void AddCodeFile(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        _compilation = _compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(SourceText.From(stream)));
    }

    /// <summary>
    /// Add assembly references to the compiler
    /// </summary>
    /// <param name="list"></param>
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

    /// <summary>
    /// Run the compiler
    /// If successful, returns the bytes for the compiled assembly
    /// If failed, returns diagnostic error information
    /// </summary>
    /// <param name="code">Optional supplied code. Code could also have been added by calls to Add</param>
    /// <returns>Result</returns>
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
    /// - First, transpiles the program into C#
    /// - Then runs the C# compiler on that source code and returns a program assembly
    /// </summary>
    /// <param name="program"></param>
    /// <param name="apiType">The Type of the API the program targets</param>
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


