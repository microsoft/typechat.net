// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

internal struct PluginFunctionName
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
        int indexOf = name.LastIndexOf(separator);
        if (indexOf < 0)
        {
            throw new ArgumentException($"Invalid PluginFunctionName {name}");
        }

        string function = null;
        if (indexOf == 0)
        {
            function = name;
            return new PluginFunctionName(function);
        }

        string pluginName = name.Substring(0, indexOf);
        string functionName = name.Substring(indexOf + separator.Length);
        return new PluginFunctionName(pluginName, functionName);
    }

}
