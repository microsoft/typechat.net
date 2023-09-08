// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Dialog;

public class Question
{
    public string Text { get; set; }

    public static implicit operator string(Question q) => q.Text;
}

public class AgentResponse<T>
{
    public bool IsDone { get; set; } = false;

    [Comment("Use this to ask questions, notify user etc")]
    public string Message { get; set; }

    [Comment("Return Value if JSON has ALL required information. Else ask questions")]
    public T? Value { get; set; }

    [Comment("Use this for text that was not understood")]
    public string[]? NotTranslated { get; set; }

    [JsonIgnore]
    public bool HasMessage => (!string.IsNullOrEmpty(Message));
}
