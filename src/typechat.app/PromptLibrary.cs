// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public class PromptLibrary
{
    public static PromptSection Now()
    {
        DateTime now = DateTime.Now;
        PromptSection section = $"Use precise date and times RELATIVE TO CURRENT DATE: {now.ToLongDateString()} CURRENT TIME: {now.ToLongTimeString()}";
        section += "Also turn ranges like next week and next month into precise dates";
        return section;
    }
}
