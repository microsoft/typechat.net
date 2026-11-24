// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// A library of common Prompt Sections and instructions
/// </summary>
public class PromptLibrary
{
    /// <summary>
    /// Tell the model about the current Date and Time
    /// </summary>
    /// <returns>prompt section</returns>
    public static PromptSection Now() => Now(DateTime.Now);

    /// <summary>Give relative-date instructions using an application-supplied local time.</summary>
    public static PromptSection Now(DateTime now)
    {
        return PromptSection.FromSystem(
            $"Use precise date and times RELATIVE TO CURRENT DATE: {now.ToLongDateString()} CURRENT TIME: {now.ToLongTimeString()}\n" +
            "Also turn ranges like next week and next month into precise dates");
    }
}
