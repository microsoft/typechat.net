// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// A Json Validator that uses the .NET Json Serializer to validate that the given json
/// conforms to the target schema
/// </summary>
public class JsonSerializerTypeValidator : IJsonTypeValidator
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

    static JsonSerializerOptions _defaultOptions = DefaultOptions();
    JsonSerializerOptions _options;

    public static JsonSerializerTypeValidator Default = new JsonSerializerTypeValidator(_defaultOptions);

    public JsonSerializerTypeValidator(JsonSerializerOptions? options = null)
    {
        options ??= _defaultOptions;
        _options = options;
    }

    public JsonSerializerOptions Options => _options;

    public Result<object?> Validate(TypeSchema schema, string json)
    {
        try
        {
            return JsonSerializer.Deserialize(json, schema.Type, _options);
        }
        catch (JsonException jex)
        {
            return Result<object?>.Error(ToErrorString(json, jex));
        }
        catch (Exception ex)
        {
            return Result<object?>.Error(ex.Message);
        }
    }

    string ToErrorString(string json, JsonException error)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("### JSON ERROR:");
        sb.AppendLineNotEmpty(error.Message);
        if (error.Path != null)
        {
            sb.Append("Property with error: ");
            sb.AppendLine(ParsePath(error.Path));
        }
        if (error.LineNumber != null)
        {
            sb.AppendLine("### Errors here:");
            json.ExtractLine((long)error.LineNumber, sb);
        }
        return sb.ToString();
    }

    string ParsePath(string path)
    {
        const string pathPrefix = "$.";
        int iPrefix = path.IndexOf(pathPrefix);
        if (iPrefix >= 0)
        {
            return path.Substring(iPrefix + pathPrefix.Length);
        }
        return path;
    }
}

public class JsonSerializerTypeValidator<T> : IJsonTypeValidator<T>
{
    TypeSchema _schema;
    JsonSerializerTypeValidator _validator;

    public JsonSerializerTypeValidator(SchemaText schema, JsonSerializerOptions? options = null)
        : this(new TypeSchema(typeof(T), schema), options)
    {
    }

    public JsonSerializerTypeValidator(TypeSchema schema, JsonSerializerOptions? options = null)
    {
        if (schema == null)
        {
            throw new ArgumentNullException(nameof(schema));
        }
        
        _schema = schema;
        if (options != null)
        {
            _validator = new JsonSerializerTypeValidator(options);
        }
        else
        {
            _validator = JsonSerializerTypeValidator.Default;
        }
    }

    public TypeSchema Schema => _schema;
    public JsonSerializerOptions Options => _validator.Options;

    public Result<T> Validate(string json)
    {
        Result<object?> result = _validator.Validate(_schema, json);
        return new Result<T>(result);
    }
}
