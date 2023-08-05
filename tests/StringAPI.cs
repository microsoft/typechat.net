// Copyright (c) Microsoft. All rights reserved.

using System.Text.Json;
namespace Microsoft.TypeChat.Tests;

public class StringAPI
{
    public AnyJsonValue HandleCall(string name, AnyJsonValue[] args)
    {
        switch (name)
        {
            default:
                throw new NotSupportedException(name);
            case "concat":
                return Concat(args);
        }

        //return AnyJsonValue.Undefined;
    }

    public static string Concat(AnyJsonValue[] args)
    {
        StringBuilder sb = new StringBuilder();
        Concat(args, sb);
        return sb.ToString();
    }

    static string Concat(AnyJsonValue value, StringBuilder sb)
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
                var obj = value.Object;
                foreach (var kv in obj)
                {
                    sb.Append('[').Append(kv.Key).Append(", ");
                    Concat(kv.Value, sb);
                    sb.Append("]");
                }
                break;
        }
        return sb.ToString();
    }

}
