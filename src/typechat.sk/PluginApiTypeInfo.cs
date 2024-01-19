// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// Type information for a Plugin API
/// This is type information returned by the semantic kernel, but cached for fast lookup
/// The type information defines the shape of the API: its methods, the signatures for the methods
/// </summary>
public class PluginApiTypeInfo : SortedList<PluginFunctionName, FunctionView>
{
    /// <summary>
    /// Use ALL Skills and Plugins registered in this kernel to initialize the Api
    /// </summary>
    /// <param name="kernel">kernel</param>
    public PluginApiTypeInfo(IKernel kernel)
        : this(kernel.Skills.GetFunctionsView())
    {
    }

    /// <summary>
    /// Create type information for the given plugins. These will form the plugin Api
    /// </summary>
    /// <param name="plugins">plugins in this API</param>
    public PluginApiTypeInfo(FunctionsView plugins = null)
        : base()
    {
        if (plugins is not null)
        {
            Add(plugins);
        }
    }

    /// <summary>
    /// Add a plugin to the Api
    /// </summary>
    /// <param name="plugins">plugin to register</param>
    public void Add(FunctionsView plugins)
    {
        ArgumentVerify.ThrowIfNull(plugins, nameof(plugins));

        if (plugins.SemanticFunctions is not null)
        {
            Add(plugins.SemanticFunctions.Values);
        }

        if (plugins.NativeFunctions is not null)
        {
            Add(plugins.NativeFunctions.Values);
        }
    }

    /// <summary>
    /// Add a collection of Skills to this Api
    /// </summary>
    /// <param name="plugins">plugins to register</param>
    public void Add(IEnumerable<IEnumerable<FunctionView>> plugins)
    {
        ArgumentVerify.ThrowIfNull(plugins, nameof(plugins));
        foreach (var plugin in plugins)
        {
            Add(plugin);
        }
    }

    /// <summary>
    /// Add a collection of semantic kernel functions to this Api
    /// </summary>
    /// <param name="plugin">plugin</param>
    public void Add(IEnumerable<FunctionView> plugin)
    {
        ArgumentVerify.ThrowIfNull(plugin, nameof(plugin));

        foreach (var function in plugin)
        {
            Add(function);
        }
    }

    /// <summary>
    /// Add a function to the Api
    /// </summary>
    /// <param name="function">function to add</param>
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
