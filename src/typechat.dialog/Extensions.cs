// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Dialog;

internal static class SerializationEx
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

    public static bool AddHistory(this PromptBuilder builder, IMessageStream history)
    {
        int historyStartAt = builder.Prompt.Count;
        bool retVal = builder.AddRange(history.Newest());
        int historyEndAt = builder.Prompt.Count;
        if (historyStartAt < historyEndAt)
        {
            builder.Prompt.Reverse(historyStartAt, historyEndAt - historyStartAt);
        }
        return retVal;
    }
}
