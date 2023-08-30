// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public struct ChatMessage
{
    public const string DefaultFrom = "User";

    string _from;
    string _text;

    [JsonConstructor]
    public ChatMessage(string text, string? from = null)
    {
        ArgumentNullException.ThrowIfNull(text, nameof(text));
        if (string.IsNullOrEmpty(from))
        {
            from = DefaultFrom;
        }
        _text = text;
        _from = from;
    }

    [JsonPropertyName("from")]
    public string From => _from;

    [JsonPropertyName("text")]
    public string Text => _text;

    public static implicit operator string(ChatMessage message) => message.Text;
    public static implicit operator ChatMessage(string text) => new ChatMessage(text);
}
