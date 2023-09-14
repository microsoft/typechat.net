// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// Metadata about individual semantic kernel plugin functions
/// Each function can be scoped by its plugin name. This eliminates name collisions
/// </summary>
public class PluginFunctionName : IEquatable<PluginFunctionName>, IComparable<PluginFunctionName>
{
    public const string DefaultSeparator = "__";

    /// <summary>
    /// A function that is not part of a Plugin
    /// </summary>
    /// <param name="functionName"></param>
    public PluginFunctionName(string functionName)
        : this(null, functionName)
    {
    }

    /// <summary>
    /// Create a PluginFunctionName
    /// </summary>
    /// <param name="pluginName">The plugin that contains this function</param>
    /// <param name="functionName">function</param>
    public PluginFunctionName(string pluginName, string functionName)
    {
        PluginName = pluginName;
        FunctionName = functionName;
    }

    /// <summary>
    /// Plugin
    /// </summary>
    public string? PluginName { get; private set; }
    /// <summary>
    /// A function contained in a plugin
    /// </summary>
    public string? FunctionName { get; private set; }

    public bool IsGlobal => (string.IsNullOrEmpty(PluginName));

    public override string ToString() => ToString(DefaultSeparator);
    /// <summary>
    /// Returns the plugin function name programs can use to reference this plugin function.
    /// By default is {pluginName}.{functionName}
    /// </summary>
    /// <param name="separator"></param>
    /// <returns></returns>
    public string ToString(string separator)
    {
        if (IsGlobal)
        {
            return FunctionName;
        }
        if (string.IsNullOrEmpty(separator))
        {
            return $"{PluginName}{FunctionName}";
        }
        return $"{PluginName}{separator}{FunctionName}";
    }
    /// <summary>
    /// Parse the plugin function string
    /// </summary>
    /// <param name="name"></param>
    /// <param name="separator"></param>
    /// <returns></returns>
    public static PluginFunctionName Parse(string name, string separator = DefaultSeparator)
    {
        string function = null;
        int indexOf = name.LastIndexOf(separator);
        if (indexOf <= 0)
        {
            function = name;
            return new PluginFunctionName(function);
        }

        string pluginName = name.Substring(0, indexOf);
        string functionName = name.Substring(indexOf + separator.Length);
        return new PluginFunctionName(pluginName, functionName);
    }

    public override bool Equals(object? obj)
    {
        return obj is PluginFunctionName name && this.Equals(name);
    }

    public bool Equals(PluginFunctionName other)
    {
        return this.PluginName == other.PluginName &&
               this.FunctionName == other.FunctionName;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.PluginName, this.FunctionName);
    }

    public int CompareTo(PluginFunctionName other)
    {
        int cmp = string.CompareOrdinal(this.PluginName, other.PluginName);
        if (cmp == 0)
        {
            cmp = string.CompareOrdinal(this.FunctionName, other.FunctionName);
        }
        return cmp;
    }

    public static bool operator ==(PluginFunctionName left, PluginFunctionName right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(PluginFunctionName left, PluginFunctionName right)
    {
        return !(left == right);
    }
}
