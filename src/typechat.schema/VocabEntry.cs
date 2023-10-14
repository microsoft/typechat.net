// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

/// <summary>
/// An Vocabulary Entry.
/// </summary>
public struct VocabEntry : IComparable<VocabEntry>, IEquatable<VocabEntry>
{
    [JsonConstructor]
    public VocabEntry(string text)
    {
        ArgumentVerify.ThrowIfNullOrEmpty(text, nameof(text));
        Text = text;
    }

    /// <summary>
    /// Vocabulary text
    /// </summary>
    public string Text { get; }

    public int CompareTo(VocabEntry other)
        => Text.CompareTo(other.Text);

    public int CompareTo(VocabEntry other, StringComparison comparison)
        => string.Compare(Text, other.Text, comparison);

    public override bool Equals(object? obj) => Text.Equals(obj);

    public bool Equals(VocabEntry other) => Text.Equals(other.Text);

    public override int GetHashCode() => Text.GetHashCode();

    public override string ToString() => Text;

    public static implicit operator string(VocabEntry entry) => entry.Text;

    public static implicit operator VocabEntry(string text) => new VocabEntry(text);
}
