// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public class Prompt : List<IPromptSection>
{
    public Prompt(PromptSection? text = null)
        : this(null, text)
    {
    }

    public Prompt(IList<IPromptSection>? preamble, PromptSection? text = null)
    {
        if (preamble != null)
        {
            EnsureCapacity(preamble.Count);
            AddRange(preamble);
        }
        if (text != null)
        {
            Add(text);
        }
    }

    public new void Add(IPromptSection section)
    {
        ArgumentNullException.ThrowIfNull(section, nameof(section));
        base.Add(section);
    }

    public void Push(string section) => Push(PromptSection.FromUser(section));
    public void PushResponse(string section) => Push(PromptSection.FromAssistant(section));

    public void Push(IPromptSection section) => Add(section);

    public IPromptSection? Last()
    {
        int position = IndexOfLast();
        if (position >= 0)
        {
            return this[position];
        }

        return null;
    }

    public IPromptSection? Pop()
    {
        int position = IndexOfLast();
        if (position >= 0)
        {
            IPromptSection? last = this[position];
            RemoveAt(position);
            return last;
        }

        return null;
    }

    public void Trim(int trimCount)
    {
        if (trimCount > Count)
        {
            Clear();
        }
        else
        {
            RemoveRange(Count - trimCount, trimCount);
        }
    }

    public void JoinSections(string sectionSep, bool includeSource, StringBuilder sb)
    {
        foreach (var section in this)
        {
            if (includeSource)
            {
                sb.Append(section.Source).Append(":\n");
            }
            sb.Append(section.GetText()).Append(sectionSep);
        }
    }

    int IndexOfLast()
    {
        return Count > 0 ? Count - 1 : -1;
    }

    public override string ToString() => ToString(true);

    public string ToString(bool includeSource)
    {
        StringBuilder sb = new StringBuilder();
        JoinSections("\n\n", includeSource, sb);
        return sb.ToString();
    }

    public static implicit operator Prompt(string text) => new Prompt(text);
    public static implicit operator Prompt(PromptSection section) => new Prompt(section);
    public static implicit operator string(Prompt prompt) => prompt.ToString();
}
