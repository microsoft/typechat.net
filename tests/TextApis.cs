// Copyright (c) Microsoft. All rights reserved.

using System.Text.Json;
namespace Microsoft.TypeChat.Tests;

public class TextApis : IStringAPI, ITimeAPI
{
    public string concat(dynamic[] args)
    {
        StringBuilder sb = new StringBuilder();
        Concat(args, sb);
        return sb.ToString();
    }

    public string uppercase(string arg) => arg.ToUpper();
    public string lowercase(string arg) => arg.ToLower();
    public string dateTime() { return DateTime.Now.ToString(); }
    public string date() { return DateTime.Now.Date.ToString(); }
    public string time() { return DateTime.Now.TimeOfDay.ToString(); }

    public string Concat(dynamic value, StringBuilder sb)
    {
        if (value is dynamic[] array)
        {
            for (int i = 0; i < array.Length; ++i)
            {
                Concat(array[i], sb);
            }
        }
        else if (value is JsonObject obj)
        {
            foreach (var kv in obj)
            {
                sb.Append('[').Append(kv.Key).Append(", ");
                sb.Append(kv.Value);
                sb.Append("]");
            }

        }
        else
        {
            sb.Append(value);
        }
        return sb.ToString();
    }

}
