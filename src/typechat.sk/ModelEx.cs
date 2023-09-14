// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

internal static class ModelEx
{
    public static AuthorRole GetRole(this IPromptSection section)
    {
        string? source = section.Source;
        if (string.IsNullOrEmpty(source))
        {
            return AuthorRole.User;
        }
        if (AuthorRole.User.IsRole(source))
        {
            return AuthorRole.User;
        }
        if (AuthorRole.Assistant.IsRole(source))
        {
            return AuthorRole.Assistant;
        }
        if (AuthorRole.System.IsRole(source))
        {
            return AuthorRole.System;
        }
        return AuthorRole.User;
    }

    public static void Append(this ChatHistory history, IEnumerable<IPromptSection> sections)
    {
        ArgumentNullException.ThrowIfNull(sections, nameof(sections));

        foreach (var section in sections)
        {
            history.Append(section);
        }
    }

    public static void Append(this ChatHistory history, IPromptSection message)
    {
        history.AddMessage(message.GetRole(), message.GetText());
    }

    internal static bool IsRole(this AuthorRole role, string label)
    {
        return role.Label.Equals(label, StringComparison.OrdinalIgnoreCase);
    }
}
