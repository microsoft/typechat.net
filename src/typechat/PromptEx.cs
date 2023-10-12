// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

internal static class PromptEx
{
    /// <summary>
    /// Get a completion for the given prompt
    /// </summary>
    /// <param name="model">model</param>
    /// <param name="prompt">prompt</param>
    /// <param name="maxTokens">maximum tokens to emit</param>
    /// <param name="temperature">response temperature</param>
    /// <returns></returns>
    public static async Task<string> CompleteAsync(this ILanguageModel model, Prompt prompt, int maxTokens = -1, double temperature = 0.0)
    {
        TranslationSettings settings = new TranslationSettings { MaxTokens = maxTokens, Temperature = temperature };
        return await model.CompleteAsync(prompt, settings, CancellationToken.None);
    }

    internal static void Trim<T>(this List<T> list, int trimCount)
        where T : IPromptSection
    {
        if (trimCount > list.Count)
        {
            list.Clear();
        }
        else
        {
            list.RemoveRange(list.Count - trimCount, trimCount);
        }
    }

}
