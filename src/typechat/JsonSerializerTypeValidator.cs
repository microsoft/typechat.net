// Copyright (c) Microsoft. All rights reserved.

using System.Text.Json;

namespace Microsoft.TypeChat;

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

    public ValidationResult<object?> Validate(TypeSchema schema, string json)
    {
        try
        {
            return JsonSerializer.Deserialize(GetJson(json), schema.Type, _options);
        }
        catch (JsonException jex)
        {
            return ValidationResult<object?>.Error(ToErrorString(json, jex));
        }
        catch (Exception ex)
        {
            return ValidationResult<object?>.Error(ex.Message);
        }
    }

    ReadOnlySpan<char> GetJson(string json)
    {
        int iStartAt = json.IndexOf('{');
        int iEndAt = json.LastIndexOf('}');
        if (iStartAt < 0 || iEndAt < 0 || iStartAt >= iEndAt)
        {
            throw new JsonException("JSON parse error");
        }
        return json.AsSpan(iStartAt, iEndAt - iStartAt + 1);
    }

    string ToErrorString(string json, JsonException error)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(error.Message);
        if (error.Path != null)
        {
            sb.Append("Property with error: ");
            sb.AppendLine(ParsePath(error.Path));
        }
        if (error.LineNumber != null)
        {
            sb.AppendLine("Line with error:");
            sb.AppendLine(json.GetLine((long)error.LineNumber));
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
        ArgumentNullException.ThrowIfNull(schema, nameof(schema));
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

    public ValidationResult<T> Validate(string json)
    {
        ValidationResult<object?> result = _validator.Validate(_schema, json);
        return new ValidationResult<T>(result);
    }
}
