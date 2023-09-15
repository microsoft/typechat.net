// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Tests.Netstandard20;

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
            if (line.Contains(subStrings.First()) && subStrings.Skip(1).All(s => line.Contains(s)))
            {
                return true;
            }
        }
        return false;
    }
}
