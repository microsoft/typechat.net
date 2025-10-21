// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// Type information for a Plugin API
/// This is type information returned by the semantic kernel, but cached for fast lookup
/// The type information defines the shape of the API: its methods, the signatures for the methods
/// </summary>
public class PluginApiTypeInfo : SortedList<PluginFunctionName, KernelFunctionMetadata>
{
    /// <summary>
    /// Use ALL Plugins registered in this kernel to initialize the Api
    /// </summary>
    /// <param name="kernel">kernel</param>
    public PluginApiTypeInfo(Kernel kernel)
        : this(kernel.Plugins.GetFunctionsMetadata())
    {
    }

    /// <summary>
    /// Use ALL Plugins registered in this kernel to initialize the Api
    /// </summary>
    /// <param name="plugins">plugins to use</param>
    public PluginApiTypeInfo(IEnumerable<KernelFunctionMetadata> plugins)
    {
        Add(plugins);
    }

    /// <summary>
    /// Add a collection of Plugins to this Api
    /// </summary>
    /// <param name="plugins">plugins to register</param>
    public void Add(IEnumerable<KernelFunctionMetadata> plugins)
    {
        if (plugins is not null)
        {
            foreach (var plugin in plugins)
            {
                Add(plugin);
            }
        }
    }

    /// <summary>
    /// Add a function to the Api
    /// </summary>
    /// <param name="function">function to add</param>
    public void Add(KernelFunctionMetadata function)
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
