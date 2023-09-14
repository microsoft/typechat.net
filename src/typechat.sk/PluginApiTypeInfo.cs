// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// Type information for a Plugin API
/// This is type information returned by the semantic kernel, but cached for fast lookup
/// </summary>
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

    /// <summary>
    /// Export the type information for this Plugin API as Typescript interfaces
    /// Language models will emit programs that call this interface
    /// </summary>
    /// <param name="apiName">Name of the Api</param>
    /// <param name="apiDescription">(Optional) Description of the API</param>
    /// <returns></returns>
    public SchemaText ExportSchema(string apiName, string apiDescription = null)
    {
        using StringWriter writer = new StringWriter();
        PluginTypescriptExporter exporter = new PluginTypescriptExporter(writer);
        exporter.Comment(apiDescription);
        exporter.Export(apiName, this);
        return new SchemaText(writer.ToString(), SchemaText.Languages.Typescript);
    }
}
