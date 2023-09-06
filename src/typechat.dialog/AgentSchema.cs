// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Dialog;

/*
public enum AgentResponseType
{
    [Comment("User request is IN scope")]
    In_Scope,
    [Comment("User request is OUT of scope")]
    Out_Of_Scope,
    [Comment("You have no response")]
    NoResponse
}

public class AgentSettings
{
    public string About;
    public string Description;
}
*/

public class Question
{
    public string Text { get; set; }
}

public class AgentResponse<T>
{
    //[Comment("Set response type by classifying message based on the Topic")]
    //public AgentResponseType Type { get; set; }

    [Comment("Question to ask if translation is not possible OR all data not yet provided")]
    public Question Question { get; set; }

    [Comment("User intent is finally translated into this")]
    public T? Value { get; set; }

    [Comment("Parts of the user request that could not be translated")]
    public string[]? NotTranslated { get; set; }
}
