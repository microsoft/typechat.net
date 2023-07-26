// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public class Json
{
    public static JsonSerializerOptions DefaultOptions()
    {
        var options = new JsonSerializerOptions
        {
            IncludeFields = true,
            IgnoreReadOnlyFields = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    public static readonly Json Default = new Json();

    JsonSerializerOptions _options;

    public Json()
    {
        _options = DefaultOptions();
    }

    public static string Stringify(object value)
    {
        return Default.Serialize(value);
    }

    public static string Stringify<T>(T value)
    {
        return Default.Serialize<T>(value);
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
