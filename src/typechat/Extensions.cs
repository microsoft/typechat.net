// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

internal static class Extensions
{
    public static string GetLine(this string json, long lineNumber)
    {
        string line;
        long i = 0;
        using StringReader reader = new StringReader(json);
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
