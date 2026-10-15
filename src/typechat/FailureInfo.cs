// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// Unvalidated output associated with a failure. Retains bounded context, not an attempt history.
/// </summary>
public class FailureInfo
{
    /// <summary>
    /// Key used to attach failure context to transport exceptions without changing their type.
    /// </summary>
    public const string ExceptionDataKey = "Microsoft.TypeChat.FailureInfo";

    private object? _value;

    /// <summary>
    /// Rejected value, when available. Check HasValue before using it, and validate it before
    /// treating it as the target type. Parsed JSON is retained as an independent JsonElement.
    /// </summary>
    public object? Value
    {
        get => _value;
        set
        {
            _value = value;
            HasValue = true;
        }
    }

    /// <summary>
    /// Distinguishes unavailable data from an explicitly supplied value, including null.
    /// </summary>
    public bool HasValue { get; private set; }

    /// <summary>
    /// JSON input supplied to a validator, when available. If supplied without a value,
    /// the translator respects the validator's decision not to provide parsed data.
    /// </summary>
    public string? InputText { get; set; }

    /// <summary>
    /// Latest model response text, when available. This can come from a later repair attempt
    /// than Value if that response couldn't be parsed.
    /// </summary>
    public string? ResponseText { get; set; }

    internal FailureInfo WithFallback(FailureInfo? fallback)
    {
        // Preserve fallback metadata unless this context has its own value or custom metadata.
        FailureInfo source = !HasValue && GetType() == typeof(FailureInfo) && fallback is not null ? fallback : this;
        var result = (FailureInfo)source.MemberwiseClone();
        if (!result.HasValue && fallback?.HasValue == true)
        {
            result.Value = fallback.Value;
        }
        result.InputText = InputText ?? fallback?.InputText;
        result.ResponseText = ResponseText ?? fallback?.ResponseText;
        return result;
    }

    internal void AttachTo(Exception exception)
    {
        exception.Data[ExceptionDataKey] = exception.Data[ExceptionDataKey] is FailureInfo existing ?
            existing.WithFallback(this) :
            WithFallback(null);
    }

    internal static FailureInfo FromJson(string? json, JsonSerializerOptions options)
    {
        var result = new FailureInfo { InputText = json };
        if (json is null)
        {
            return result;
        }
        try
        {
            using JsonDocument document = JsonDocument.Parse(json, new JsonDocumentOptions
            {
                AllowTrailingCommas = options.AllowTrailingCommas,
                CommentHandling = options.ReadCommentHandling,
                MaxDepth = options.MaxDepth
            });
            result.Value = document.RootElement.Clone();
        }
        catch (JsonException)
        {
            // The original validation error reports malformed JSON; only input text is available.
        }
        return result;
    }
}
