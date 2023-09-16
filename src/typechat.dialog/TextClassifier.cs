// Copyright (c) Microsoft. All rights reserved.
using Microsoft.TypeChat.Schema;

namespace Microsoft.TypeChat.Dialog;

/// <summary>
/// A Text classifier is a JsonTranslator that translates the user's request into a TextClassification
/// The language model is provided with a set of classes to choose from
/// This is very useful for common routing problems: to categorize/bucket a request
/// A text class is a {ClassName, Description} pair
/// </summary>
public class TextClassifier : JsonTranslator<TextClassification>
{
    TextClasses _classes;

    /// <summary>
    /// Create a classifier that will make classification decisions using the given language model
    /// </summary>
    /// <param name="languageModel">model to use</param>
    public TextClassifier(ILanguageModel languageModel)
        : this(languageModel, new TextClasses())
    {

    }

    /// <summary>
    /// Create a classifier that will make classification decisions using the given language model
    /// </summary>
    /// <param name="languageModel">model to use</param>
    /// <param name="classes">Classes to classify into</param>
    public TextClassifier(ILanguageModel languageModel, TextClasses classes)
        : base(languageModel, new TypeValidator<TextClassification>(classes.Vocabs))
    {
        if (classes == null)
        {
            throw new ArgumentNullException(nameof(classes));
        }
        _classes = classes;
    }

    /// <summary>
    /// Text classes used by this classifier
    /// </summary>
    public TextClasses Classes => _classes;

    protected override Prompt CreateRequestPrompt(Prompt request, IList<IPromptSection> preamble)
    {
        string classes = Json.Stringify(_classes);
        string fullRequest = $"Classify \"{request}\" using the following classification table:\n{classes}\n";
        return base.CreateRequestPrompt(fullRequest, preamble);
    }
}

/// <summary>
/// A Text Classification Response from the Translator
/// </summary>
public class TextClassification
{
    /// <summary>
    /// The class assigned to the request
    /// </summary>
    [JsonPropertyName("class")]
    [JsonVocab(Name = "Classes", PropertyName = "class")]
    [Comment("Use this for the classification")]
    public string Class { get; set; }

    public override string ToString() => Class;
}

/// <summary>
/// A candidate class
/// </summary>
public struct TextClass
{
    /// <summary>
    /// Class name
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// Class description
    /// The description of a class has strong influence on how the model classifies a user's input
    /// </summary>
    public string Description { get; set; }

    public override string ToString()
    {
        return $"{Name}: {Description}";
    }
}

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
        Add(new TextClass { Name = name, Description = description });
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

