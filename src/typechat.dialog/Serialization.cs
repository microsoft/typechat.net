// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Dialog;

public interface ITextSerializable
{
    string Stringify();
}

public static class SerializationEx
{
    public static string Stringify(this object obj)
    {
        if (obj is string str)
        {
            return str;
        }
        if (obj is ITextSerializable textSerializable)
        {
            return textSerializable.Stringify();
        }
        return Json.Stringify(obj);
    }
}
