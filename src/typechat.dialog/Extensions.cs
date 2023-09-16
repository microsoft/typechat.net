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
    /// Add messages in priority order to the prompt
    /// Will keep adding messages until the prompt runs out of room
    /// </summary>
    /// <param name="builder">builder used to build prompt</param>
    /// <param name="history">message history to add</param>
    /// <returns></returns>
    public static bool AddHistory(this PromptBuilder builder, IEnumerable<IPromptSection> context)
    {
        int contextStartAt = builder.Prompt.Count;
        bool retVal = builder.AddRange(context);
        int contextEndAt = builder.Prompt.Count;
        if (contextStartAt < contextEndAt)
        {
            builder.Prompt.Reverse(contextStartAt, contextEndAt - contextStartAt);
        }
        return retVal;
    }
}
