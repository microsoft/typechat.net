// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// Exports Type information about Plugins as Typescript interfaces
/// This interface is a concise and precise way to communicate this information to the model
/// </summary>
public class PluginTypescriptExporter
{
    private readonly TypescriptWriter _tsWriter;

    public PluginTypescriptExporter(TextWriter writer)
    {
        _tsWriter = new TypescriptWriter(writer);
    }

    /// <summary>
    /// Include parameter descriptions in the schema
    /// Default is true
    /// </summary>
    public bool IncludeParamDescriptions { get; set; } = true;

    /// <summary>
    /// Add helper comments to the generated schema
    /// </summary>
    /// <param name="comment">comment to add</param>
    public void Comment(string comment) => _tsWriter.Comment(comment);

    /// <summary>
    /// Exports with the api with the given name and type information
    /// </summary>
    /// <param name="apiName">The name of the api</param>
    /// <param name="typeInfo">type information for this plugin Api</param>
    public void Export(string apiName, PluginApiTypeInfo typeInfo)
    {
        _tsWriter.BeginInterface(apiName);
        {
            _tsWriter.PushIndent();
            foreach (var plugin in typeInfo)
            {
                Export(plugin.Key, plugin.Value);
            }
            _tsWriter.PopIndent();
        }
        _tsWriter.EndInterface();
        _tsWriter.Flush();
    }

    private void Export(PluginFunctionName pluginName, KernelFunctionMetadata function)
    {
        ArgumentVerify.ThrowIfNull(function, nameof(function));

        _tsWriter.SOL().Comment(function.Description);
        _tsWriter.BeginMethodDeclare(pluginName.ToString());
        {
            Export(function.Parameters);
        }
        _tsWriter.EndMethodDeclare(Typescript.Types.String);
    }

    private void Export(IReadOnlyList<KernelParameterMetadata> parameters)
    {
        if (parameters is null)
        {
            return;
        }
        if (IncludeParamDescriptions && HasDescriptions(parameters))
        {
            ExportDetailed(parameters);
        }
        else
        {
            ExportPlain(parameters);
        }
    }

    private void ExportDetailed(IReadOnlyList<KernelParameterMetadata> parameters)
    {
        _tsWriter.PushIndent();
        _tsWriter.EOL();
        for (int i = 0; i < parameters.Count; ++i)
        {
            var param = parameters[i];
            if (param.HasDescription())
            {
                _tsWriter.SOL().Comment(param.Description);
            }
            if (param.IsNullable())
            {
                _tsWriter.SOL().Comment($"Default: {param.DefaultValue}");
            }
            _tsWriter.SOL();
            Export(param, i, parameters.Count);
            _tsWriter.EOL();
        }
        _tsWriter.PopIndent();
        _tsWriter.SOL();
    }

    private void ExportPlain(IReadOnlyList<KernelParameterMetadata> parameters)
    {
        for (int i = 0; i < parameters.Count; ++i)
        {
            Export(parameters[i], i, parameters.Count);
        }
    }

    private void Export(KernelParameterMetadata param, int argNumber, int argCount)
    {
        bool isArray = param.ParameterType.IsArray;
        bool isNullable = param.IsNullable();
        _tsWriter.Parameter(param.Name, DataType(param.ParameterType), argNumber, argCount, isArray, isNullable);
    }

    private bool HasDescriptions(IReadOnlyList<KernelParameterMetadata> parameters)
    {
        for (int i = 0; i < parameters.Count; ++i)
        {
            if (parameters[i].HasDescription())
            {
                return true;
            }
        }
        return false;
    }

    private string DataType(Type type)
    {
        string? dataType = Typescript.Types.ToPrimitive(type);
        if (dataType is null)
        {
            dataType = Typescript.Types.Any;
        }
        return dataType;
    }

    private PluginTypescriptExporter SOL() { _tsWriter.SOL(); return this; }
    private PluginTypescriptExporter EOL() { _tsWriter.EOL(); return this; }
}
