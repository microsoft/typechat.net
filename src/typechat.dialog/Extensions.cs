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
        return Json.Stringify(obj, false);
    }

    /// <summary>
    /// Add the newest messages in the history to the prompt
    /// </summary>
    /// <param name="builder">builder used to build prompt</param>
    /// <param name="history">message history to add</param>
    /// <returns></returns>
    public static bool AddHistory(this PromptBuilder builder, IMessageStream history)
    {
        ArgumentNullException.ThrowIfNull(history, nameof(history));

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
