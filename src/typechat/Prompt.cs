// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public class Prompt : List<IPromptSection>
{
    public Prompt(int capacity)
        : base(capacity)
    {
    }

    public Prompt(PromptSection? text = null)
        : this(null, text, null)
    {
    }

    public Prompt(IEnumerable<IPromptSection>? preamble, PromptSection? text = null, IEnumerable<IPromptSection>? postamble = null)
    {
        if (preamble != null)
        {
            AddRange(preamble);
        }
        if (text != null)
        {
            Add(text);
        }
        if (postamble != null)
        {
            AddRange(postamble);
        }
    }

    public new void Add(IPromptSection section)
    {
        ArgumentNullException.ThrowIfNull(section, nameof(section));
        base.Add(section);
    }

    public void Push(string source, string section)
    {
        Add(new PromptSection(source, section));
    }
    public void Push(string section) => Push(PromptSection.Sources.User, section);
    public void PushResponse(string section) => Push(PromptSection.Sources.Assistant, section);
    public void PushInstruction(string section) => Push(PromptSection.Sources.User, section);
    public void Push(IPromptSection section) => Add(section);
    public void Push(IEnumerable<IPromptSection> prompts) => base.AddRange(prompts);
    public void Push(Prompt prompt) => base.AddRange(prompt);

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

    public override string ToString() => ToString(false);

    public string ToString(bool includeSource)
    {
        StringBuilder sb = new StringBuilder();
        JoinSections("\n", includeSource, sb);
        return sb.ToString();
    }

    public int GetLength()
    {
        int total = 0;
        foreach (var section in this)
        {
            total += section.GetText().Length;
        }
        return total;
    }

    int IndexOfLast()
    {
        return Count > 0 ? Count - 1 : -1;
    }

    public static implicit operator Prompt(string text) => new Prompt(text);
    public static implicit operator string(Prompt prompt) => prompt.ToString();

    public static Prompt operator +(Prompt prompt, PromptSection section)
    {
        prompt.Push(section);
        return prompt;
    }
}
