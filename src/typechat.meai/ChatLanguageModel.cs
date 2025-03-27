// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Extensions.AI;

namespace Microsoft.TypeChat;

/// <summary>
/// A LanguageModel that uses IChatClient
/// </summary>
public class ChatLanguageModel : ILanguageModel
{
    private readonly IChatClient _chatClient;
    private readonly ModelInfo _model;


    /// <summary>
    /// Create a new language model using the supplied IChatClient chatClient
    /// </summary>
    /// <param name="chatClient"></param>
    /// <param name="model"></param>
    public ChatLanguageModel(IChatClient chatClient, ModelInfo model)
    {
        ArgumentVerify.ThrowIfNull(chatClient, nameof(chatClient));
        _chatClient = chatClient;
        _model = model;
    }

    /// <summary>
    /// Information about the model
    /// </summary>
    public ModelInfo ModelInfo => _model;

    /// <summary>
    /// Return a completion for the prompt
    /// </summary>
    /// <param name="prompt"></param>
    /// <param name="settings"></param>
    /// <param name="cancelToken"></param>
    /// <returns></returns>
    public async Task<string> CompleteAsync(Prompt prompt, TranslationSettings? settings = null, CancellationToken cancelToken = default)
    {
        List<ChatMessage> history = ToHistory(prompt);
        ChatOptions? options = ToRequestSettings(settings);
        var response = await _chatClient.GetResponseAsync(history, options: options, cancellationToken: cancelToken).ConfigureAwait(false);

        return response.Text;
    }

    private List<ChatMessage> ToHistory(Prompt prompt)
    {
        List<ChatMessage> history = [];
        history.Append(prompt);
        return history;
    }

    private ChatOptions? ToRequestSettings(TranslationSettings? settings)
    {
        if (settings is null)
        {
            return null;
        }

        var requestSettings = new ChatOptions();
        if (settings.Temperature >= 0)
        {
            requestSettings.Temperature = (float)settings.Temperature;
        }
        if (settings.MaxTokens > 0)
        {
            requestSettings.MaxOutputTokens = settings.MaxTokens;
        }
        return requestSettings;
    }
}

internal static class ModelEx
{
    public static ChatRole GetRole(this IPromptSection section)
    {
        string? source = section.Source;
        if (string.IsNullOrEmpty(source))
        {
            return ChatRole.User;
        }

        if (ChatRole.User.IsRole(source))
        {
            return ChatRole.User;
        }
        if (ChatRole.Assistant.IsRole(source))
        {
            return ChatRole.Assistant;
        }
        if (ChatRole.System.IsRole(source))
        {
            return ChatRole.System;
        }
        return ChatRole.User;
    }

    public static void Append(this List<ChatMessage> history, IEnumerable<IPromptSection> sections)
    {
        ArgumentVerify.ThrowIfNull(sections, nameof(sections));

        foreach (var section in sections)
        {
            history.Append(section);
        }
    }

    public static void Append(this List<ChatMessage> history, IPromptSection message)
    {
        history.Add(new ChatMessage(role: message.GetRole(), content: message.GetText()));
    }

    internal static bool IsRole(this ChatRole role, string label)
    {
        return role.Value.Equals(label, StringComparison.OrdinalIgnoreCase);
    }
}
