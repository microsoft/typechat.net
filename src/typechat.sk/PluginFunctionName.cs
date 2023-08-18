// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public struct PluginFunctionName : IEquatable<PluginFunctionName>, IComparable<PluginFunctionName>
{
    public const string DefaultSeparator = "__";

    public PluginFunctionName(string functionName)
        : this(null, functionName)
    {
    }

    public PluginFunctionName(string pluginName, string functionName)
    {
        PluginName = pluginName;
        FunctionName = functionName;
    }

    public string? PluginName { get; private set; }
    public string? FunctionName { get; private set; }

    public bool IsGlobal => (string.IsNullOrEmpty(PluginName));

    public override string ToString() => ToString(DefaultSeparator);

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
