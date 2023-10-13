// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

/// <summary>
/// A vocabulary with a name.
/// Name Vocabs can be shallow compared by their names. 
/// </summary>
public class NamedVocab : IComparable<NamedVocab>
{
    public NamedVocab(IVocab vocab)
        : this(string.Empty, vocab)
    {
    }

    public NamedVocab(string name, IVocab vocab)
    {
        ArgumentVerify.ThrowIfNull(name, nameof(name));
        ArgumentVerify.ThrowIfNull(vocab, nameof(vocab));

        Name = name;
        Vocab = vocab;
    }

    [JsonConstructor]
    public NamedVocab(string name, Vocab vocab)
        : this(name, (IVocab)vocab)
    {
    }

    /// <summary>
    /// The name of this vocab
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The vocabulary associated with this vocab
    /// </summary>
    public IVocab Vocab { get; }

    /// <summary>
    /// Compares the vocab name
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public int CompareTo(NamedVocab? other)
    {
        return Name.CompareTo(other.Name);
    }

    /// <summary>
    /// Checks if obj matches the vocab name
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object? obj) => Name.Equals(obj);

    /// <summary>
    /// Hashcode for the vocab name
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode() => Name.GetHashCode();

    /// <summary>
    /// Return the vocab name
    /// </summary>
    /// <returns></returns>
    public override string? ToString() => Name;
}
