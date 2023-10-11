// Copyright (c) Microsoft. All rights reserved.

using Microsoft.TypeChat.Validation;

namespace Microsoft.TypeChat.Tests;

internal static class Extensions
{
    public static IEnumerable<string> ReadLines(this string text)
    {
        using var reader = new StringReader(text);
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            yield return line;
        }
    }

    public static List<string> Lines(this string text)
    {
        return text.ReadLines().ToList();
    }

    public static bool ContainsSubstring(this IEnumerable<string> lines, params string[] subStrings)
    {
        foreach (var line in lines)
        {
            if (line.Contains(subStrings))
            {
                return true;
            }
        }
        return false;
    }

    public static bool Contains(this string text, params string[] subStrings)
    {
        for (int i = 0; i < subStrings.Length; ++i)
        {
            if (!text.Contains(subStrings[i]))
            {
                return false;
            }
        }
        return true;
    }

    public static JsonTranslator<T> CreateTranslator<T>(this Config config)
    {
        return new JsonTranslator<T>(
            new LanguageModel(config.OpenAI),
            new TypeValidator<T>(TestVocabs.All()));
    }

    /// <summary>
    /// Generate an array of random floats
    /// </summary>
    public static float[] FloatArray(this Random random, int count)
    {
        float[] array = new float[count];
        for (int i = 0; i < count; ++i)
        {
#if NET6_0_OR_GREATER
            array[i] = random.NextSingle();
#else
            array[i] = (float)random.NextDouble();
#endif
        }

        return array;
    }


    public static int RoundToInt(this double value)
    {
        return (int)(value + 0.5);
    }
}
