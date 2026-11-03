// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// Normalized reason a completion stopped generating, mapped from the underlying API so callers can
/// detect conditions such as truncation (<see cref="Length"/>) or filtered output
/// (<see cref="ContentFilter"/>) without special-casing each API variant.
/// </summary>
public enum CompletionFinishReason
{
    /// <summary>The model stopped naturally (Chat Completions "stop" / Responses "completed").</summary>
    Stop,

    /// <summary>Output was truncated by a token limit (Chat "length" / Responses "max_output_tokens").</summary>
    Length,

    /// <summary>Output was withheld or truncated by a content filter.</summary>
    ContentFilter,

    /// <summary>The model emitted tool/function calls.</summary>
    ToolCalls,

    /// <summary>A provider-specific reason that doesn't map to one of the common values.</summary>
    Other,
}
