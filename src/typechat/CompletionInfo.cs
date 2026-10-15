// Copyright (c) Microsoft. All rights reserved.

using System.Text.Json;

namespace Microsoft.TypeChat;

/// <summary>
/// Metadata about a single completion. Translation results carry the final completion's metadata,
/// on success or validation failure. Usage does not include earlier repair attempts.
/// </summary>
public class CompletionInfo
{
    /// <summary>
    /// The model that actually produced the completion, as reported by the API. This can differ from
    /// the requested model (for example, when an alias resolves to a dated model version).
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Normalized token usage for the request, when the API reports it.
    /// </summary>
    public TokenUsage? Usage { get; set; }

    /// <summary>
    /// Normalized reason the completion stopped, when the API reports it.
    /// </summary>
    public CompletionFinishReason? FinishReason { get; set; }

    /// <summary>
    /// Number of repair attempts TypeChat made before producing this result (0 = succeeded on the
    /// first attempt). Populated by <see cref="JsonTranslator{T}"/>, not by the language model.
    /// </summary>
    public int? RepairAttempts { get; set; }

    /// <summary>
    /// The raw, unmodified response body returned by the API, when available. Use this to access
    /// provider- or API-specific fields that are not normalized onto the other properties.
    /// </summary>
    public JsonElement? Raw { get; set; }

    internal CompletionInfo WithRepairAttempts(int repairAttempts)
    {
        var result = (CompletionInfo)MemberwiseClone();
        result.RepairAttempts = repairAttempts;
        return result;
    }
}
