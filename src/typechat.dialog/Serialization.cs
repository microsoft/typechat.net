// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public interface ITextSerializable
{
    string AsText();
}

public static class SerializationEx
{
    public static string AsText(this object obj)
    {
        if (obj is string str)
        {
            return str;
        }
        if (obj is ITextSerializable textSerializable)
        {
            return textSerializable.AsText();
        }
        return obj.ToString();
    }

    public static string AsText<T>(this T obj)
    {
        if (obj is ITextSerializable textSerializable)
        {
            return textSerializable.AsText();
        }
        return obj.ToString();
    }
}
