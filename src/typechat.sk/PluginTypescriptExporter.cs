// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel.SkillDefinition;
using Microsoft.TypeChat.Schema;

namespace Microsoft.TypeChat;

/// <summary>
/// Exports Type information about Plugins as Typescript interfaces
/// This interface is a concise and precise way to communicate this information to the model
/// </summary>
public class PluginTypescriptExporter
{
    TypescriptWriter _tsWriter;

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

    void Export(PluginFunctionName pluginName, FunctionView function)
    {
        ArgumentVerify.ThrowIfNull(function, nameof(function));

        _tsWriter.SOL().Comment(function.Description);
        _tsWriter.BeginMethodDeclare(pluginName.ToString());
        {
            Export(function.Parameters);
        }
        _tsWriter.EndMethodDeclare(Typescript.Types.String);
    }

    void Export(IList<ParameterView> parameters)
    {
        if (parameters == null)
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

    void ExportDetailed(IList<ParameterView> parameters)
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

    void ExportPlain(IList<ParameterView> parameters)
    {
        for (int i = 0; i < parameters.Count; ++i)
        {
            Export(parameters[i], i, parameters.Count);
        }
    }

    void Export(ParameterView param, int argNumber, int argCount)
    {
        bool isArray = (param.Type == ParameterViewType.Array);
        bool isNullable = param.IsNullable();
        _tsWriter.Parameter(param.Name, DataType(param.Type), argNumber, argCount, isArray, isNullable);
    }

    bool HasDescriptions(IList<ParameterView> parameters)
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

    string DataType(ParameterViewType? pType)
    {
        string type = Typescript.Types.String;
        if (pType != null)
        {
            if (pType == ParameterViewType.String)
            {
                type = Typescript.Types.String;
            }
            else if (pType == ParameterViewType.Number)
            {
                type = Typescript.Types.Number;
            }
            else if (pType == ParameterViewType.Boolean)
            {
                type = Typescript.Types.Boolean;
            }
            else if (pType == ParameterViewType.Object)
            {
                type = Typescript.Types.Any;
            }
        }
        return type;
    }

    PluginTypescriptExporter SOL() { _tsWriter.SOL(); return this; }
    PluginTypescriptExporter EOL() { _tsWriter.EOL(); return this; }
}
