// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public class Json
{
    public static JsonSerializerOptions DefaultOptions()
    {
        return JsonSerializerTypeValidator.DefaultOptions();
    }

    public static readonly Json Default = new Json();
    public static readonly Json Indented = new Json(true);

    JsonSerializerOptions _options;

    public Json(bool indented = false)
    {
        _options = DefaultOptions();
        _options.WriteIndented = indented;
    }

    public static string Stringify(object value, bool indented = true)
    {
        return indented ?
               Indented.Serialize(value) :
               Default.Serialize(value);
    }

    public static string Stringify<T>(T value, bool indented = true)
    {
        return indented ?
               Indented.Serialize(value) :
               Default.Serialize<T>(value);
    }

    public static object? Parse(string json, Type type)
    {
        return Default.Deserialize(json, type);
    }

    public static T Parse<T>(string json)
    {
        return (T)Parse(json, typeof(T));
    }

    string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize<T>(value, _options);
    }

    object? Deserialize(string json, Type type)
    {
        return JsonSerializer.Deserialize(json, type, _options);
    }
}
