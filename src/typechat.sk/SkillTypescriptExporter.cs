// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel.SkillDefinition;

namespace Microsoft.TypeChat.SemanticKernel;

public class SkillTypescriptExporter
{
    TypescriptWriter _tsWriter;

    public SkillTypescriptExporter(TextWriter writer)
    {
        _tsWriter = new TypescriptWriter(writer);
    }

    public bool IncludeParamDescriptions { get; set; } = false;

    public void Export(string interfaceName, IEnumerable<IEnumerable<FunctionView>> skills)
    {
        _tsWriter.BeginInterface(interfaceName);
        {
            _tsWriter.PushIndent();
            foreach (var skill in skills)
            {
                Export(skill);
            }
            _tsWriter.PopIndent();
        }
        _tsWriter.EndInterface();
        _tsWriter.Writer.Flush();
    }

    void Export(IEnumerable<FunctionView> functions)
    {
        foreach (var fview in functions)
        {
            Export(fview);
        }
    }

    void Export(FunctionView fview)
    {
        string methodname = fview.IsGlobal() ?
                            fview.Name :
                            $"{fview.SkillName}_{fview.Name}";

        _tsWriter.SOL().Comment(fview.Description);
        _tsWriter.BeginMethodDeclare(methodname);
        {
            Export(fview.Parameters);
        }
        _tsWriter.EndMethodDeclare();
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
}
