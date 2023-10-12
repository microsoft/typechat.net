// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Classification;

/// <summary>
/// The collection of possible classes
/// </summary>
public class TextClasses
{
    public const string VocabName = "Classes";

    List<TextClass> _classes;
    VocabCollection _vocabs;
    Vocab _vocab;

    /// <summary>
    /// Create a new set of classes
    /// </summary>
    public TextClasses()
    {
        _classes = new List<TextClass>();
        _vocabs = new VocabCollection();
        _vocab = new Vocab();
        _vocabs.Add(VocabName, _vocab);
    }

    /// <summary>
    /// Registered classes
    /// </summary>
    public IReadOnlyList<TextClass> Classes => _classes;

    internal IVocabCollection Vocabs => _vocabs;

    /// <summary>
    /// Add a new class 
    /// </summary>
    /// <param name="name">the name of the class</param>
    /// <param name="description">the description of the class</param>
    public void Add(string name, string description)
    {
        Add(new TextClass(name, description));
    }

    /// <summary>
    /// Add a text class to the classifier
    /// </summary>
    /// <param name="cls">new text class</param>
    public void Add(TextClass cls)
    {
        _classes.Add(cls);
        _vocab.Add(cls.Name);
    }
}

