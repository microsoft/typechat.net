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

    [Comment("Ask questions if more information is needed for complete translation")]
    public Question Question { get; set; }

    [Comment("JSON object that has ALL required information")]
    public T? Value { get; set; }

    [Comment("Parts of the user request that could not be translated")]
    public string[]? NotTranslated { get; set; }

    [JsonIgnore]
    public bool HasQuestion => (Question != null && !string.IsNullOrEmpty(Question.Text));
}
