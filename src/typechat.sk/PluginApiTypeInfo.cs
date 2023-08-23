// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public class PluginApiTypeInfo : SortedList<PluginFunctionName, FunctionView>
{
    public PluginApiTypeInfo(IKernel kernel)
        : this(kernel.Skills.GetFunctionsView())
    {
    }

    public PluginApiTypeInfo(FunctionsView plugins = null)
        : base()
    {
        if (plugins != null)
        {
            Add(plugins);
        }
    }

    public void Add(FunctionsView plugins)
    {
        ArgumentNullException.ThrowIfNull(plugins, nameof(plugins));

        if (plugins.SemanticFunctions != null)
        {
            Add(plugins.SemanticFunctions.Values);
        }
        if (plugins.NativeFunctions != null)
        {
            Add(plugins.NativeFunctions.Values);
        }
    }

    public void Add(IEnumerable<IEnumerable<FunctionView>> plugins)
    {
        ArgumentNullException.ThrowIfNull(plugins, nameof(plugins));
        foreach (var plugin in plugins)
        {
            Add(plugin);
        }
    }

    public void Add(IEnumerable<FunctionView> plugin)
    {
        ArgumentNullException.ThrowIfNull(plugin, nameof(plugin));

        foreach (var function in plugin)
        {
            Add(function);
        }
    }

    public void Add(FunctionView function)
    {
        Add(function.ToPluginName(), function);
    }

    public SchemaText ExportSchema(string apiName, string apiDescription = null)
    {
        using StringWriter writer = new StringWriter();
        PluginTypescriptExporter exporter = new PluginTypescriptExporter(writer);
        exporter.Comment(apiDescription);
        exporter.Export(apiName, this);
        return new SchemaText(writer.ToString());
    }
}
