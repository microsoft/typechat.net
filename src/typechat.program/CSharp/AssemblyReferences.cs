// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.CSharp;

/// <summary>
/// A unique set of references for an assembly
/// </summary>
public class AssemblyReferences : HashSet<string>
{
    /// <summary>
    /// Automatically adds references to standard .NET assemblies
    /// </summary>
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

    /// <summary>
    /// Add a reference to an named assembly located in the provided root folder
    /// </summary>
    /// <param name="rootPath"></param>
    /// <param name="assemblyName"></param>
    /// <exception cref="ArgumentException"></exception>
    public void Add(string rootPath, string assemblyName)
    {
        string assemblyPath = Path.Combine(rootPath, assemblyName);

        if (!File.Exists(assemblyPath))
        {
            throw new ArgumentException($"{assemblyPath} not found");
        }

        _ = Add(assemblyPath);
    }

    /// <summary>
    /// Add a reference to the assembly for the given Type
    /// </summary>
    /// <param name="types"></param>
    public void Add(params Type[] types)
    {
        foreach (Type t in types)
        {
            _ = Add(t.Assembly.Location);
        }
    }
}


