// Copyright (c) Microsoft. All rights reserved.

using System.Text.Json;
namespace Microsoft.TypeChat.Tests;

public interface IStringAPI
{
    string concat(AnyJsonValue[] args);
    string uppercase(string text);
}
public class StringAPI : IStringAPI
{
    public string concat(AnyJsonValue[] args)
    {
        StringBuilder sb = new StringBuilder();
        Concat(args, sb);
        return sb.ToString();
    }

    public string uppercase(string arg)
    {
        return arg.ToUpper();
    }

    public string lowercase(string arg)
    {
        return arg.ToLower();
    }

    public string Concat(AnyJsonValue value, StringBuilder sb)
    {
        switch (value.Type)
        {
            default:
                sb.Append(value.ToString());
                break;

            case JsonValueKind.Array:
                var array = value.Array;
                for (int i = 0; i < array.Length; ++i)
                {
                    Concat(array[i], sb);
                }
                break;
            case JsonValueKind.Object:
                var obj = value.JsonObject;
                foreach (var kv in obj)
                {
                    sb.Append('[').Append(kv.Key).Append(", ");
                    sb.Append(kv.Value);
                    sb.Append("]");
                }
                break;
        }
        return sb.ToString();
    }

}
