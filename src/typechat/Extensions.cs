// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

internal static class Extensions
{
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
