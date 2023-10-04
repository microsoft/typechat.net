// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public static class Extensions
{
    public static bool IsNullOrEmpty<T>(this IList<T> list)
    {
        return (list == null || list.Count == 0);
    }

    /// <summary>
    /// Get a completion for the given prompt
    /// </summary>
    /// <param name="model">model</param>
    /// <param name="prompt">prompt</param>
    /// <param name="maxTokens">maximum tokens to emit</param>
    /// <param name="temperature">respose temperature</param>
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

    internal static void ExtractLine(this string text, long lineNumber, StringBuilder sb)
    {
        string line;
        long i = 0;
        long iPrev = lineNumber - 1;
        long iNext = lineNumber + 1;
        using StringReader reader = new StringReader(text);
        while ((line = reader.ReadLine()) != null)
        {
            if (i == iPrev ||
                i == lineNumber ||
                i == iNext)
            {
                sb.TrimAndAppendLine(line);
                if (i == iNext)
                {
                    break;
                }
            }
            ++i;
        }
    }

    internal static void AppendLineNotEmpty(this StringBuilder sb, string line)
    {
        if (!string.IsNullOrEmpty(line))
        {
            sb.AppendLine(line);
        }
    }

    internal static void TrimAndAppendLine(this StringBuilder sb, string line)
    {
        sb.AppendLineNotEmpty(line.Trim());
    }
}
