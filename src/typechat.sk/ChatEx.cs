// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public static class ChatEx
{
    public static bool IsRole(this AuthorRole role, string label)
    {
        return role.Label.Equals(label, StringComparison.OrdinalIgnoreCase);
    }

    public static AuthorRole GetRole(this ChatMessage message)
    {
        string? from = message.From;
        if (string.IsNullOrEmpty(from))
        {
            return AuthorRole.User;
        }
        if (AuthorRole.User.IsRole(from))
        {
            return AuthorRole.User;
        }
        if (AuthorRole.Assistant.IsRole(from))
        {
            return AuthorRole.Assistant;
        }
        if (AuthorRole.System.IsRole(from))
        {
            return AuthorRole.System;
        }
        return AuthorRole.User;
    }

    public static void Append(this ChatHistory history, IEnumerable<ChatMessage> messages)
    {
        ArgumentNullException.ThrowIfNull(messages, nameof(messages));

        foreach (var message in messages)
        {
            history.Append(message);
        }
    }

    public static void Append(this ChatHistory history, ChatMessage message)
    {
        history.AddMessage(message.GetRole(), message.Text);
    }
}
