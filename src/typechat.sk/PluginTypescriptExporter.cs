// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel.SkillDefinition;

namespace Microsoft.TypeChat;

public class PluginTypescriptExporter
{
    TypescriptWriter _tsWriter;

    public PluginTypescriptExporter(TextWriter writer)
    {
        _tsWriter = new TypescriptWriter(writer);
    }

    public bool IncludeParamDescriptions { get; set; } = false;

    public void Comment(string descr) => _tsWriter.Comment(descr);

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
        _tsWriter.Writer.Flush();
    }

    void Export(PluginFunctionName pluginName, FunctionView function)
    {
        ArgumentNullException.ThrowIfNull(function, nameof(function));

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
            _tsWriter.PushIndent();
            _tsWriter.EOL();
            for (int i = 0; i < parameters.Count; ++i)
            {
                var param = parameters[i];
                if (param.HasDescription())
                {
                    _tsWriter.SOL().Comment(param.Description);
                }
                _tsWriter.SOL();
                Export(param, i, parameters.Count);
                _tsWriter.EOL();
            }
            _tsWriter.PopIndent();
            _tsWriter.SOL();
        }
        else
        {
            for (int i = 0; i < parameters.Count; ++i)
            {
                Export(parameters[i], i, parameters.Count);
            }
        }
    }

    void Export(ParameterView param, int argNumber, int argCount)
    {
        bool isArray = (param.Type == ParameterViewType.Array);
        _tsWriter.Parameter(param.Name, DataType(param.Type), argNumber, argCount, isArray);
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
