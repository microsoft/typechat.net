// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

internal static class Extensions
{
    public static string GetLine(this string text, long lineNumber)
    {
        string line;
        long i = 1;
        using StringReader reader = new StringReader(text);
        while ((line = reader.ReadLine()) != null)
        {
            if (i == lineNumber)
            {
                line = line.Trim();
                break;
            }
            ++i;
        }
        return line;
    }
}
