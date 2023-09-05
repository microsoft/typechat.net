// Copyright (c) Microsoft. All rights reserved.
using Microsoft.TypeChat.Schema;

namespace Microsoft.TypeChat;

public class TextClassification
{
    [JsonPropertyName("class")]
    [JsonVocab(Name = "Classes", PropertyName = "class")]
    public string Class { get; set; }

    public override string ToString() => Class;
}

public struct TextClass
{
    public string Name { get; set; }
    public string Description { get; set; }

    public override string ToString()
    {
        return $"{Name}: {Description}";
    }
}

public class TextClasses : List<TextClass>
{
    public const string VocabName = "Classes";

    VocabCollection _vocabs;
    Vocab _vocab;

    public TextClasses()
    {
        _vocabs = new VocabCollection();
        _vocab = new Vocab();
        _vocabs.Add(VocabName, _vocab);
    }

    public IVocabCollection Vocabs => _vocabs;

    public void Add(string name, string description)
    {
        base.Add(new TextClass { Name = name, Description = description });
        _vocab.Add(name);
    }
}

/// <summary>
/// A simple text classifier
/// Translates user requests into the closest applicable class
/// </summary>
public class TextClassifier : JsonTranslator<TextClassification>
{
    TextClasses _classes;

    public TextClassifier(ILanguageModel languageModel)
        : this(languageModel, new TextClasses())
    {

    }

    public TextClassifier(ILanguageModel languageModel, TextClasses classes)
        : base(languageModel, new TypeValidator<TextClassification>(classes.Vocabs))
    {
        ArgumentNullException.ThrowIfNull(classes, nameof(classes));
        _classes = classes;
    }

    public TextClasses Classes => _classes;

    protected override Prompt CreateRequestPrompt(string request, IEnumerable<IPromptSection> preamble)
    {
        string classes = Json.Stringify(_classes);
        string fullRequest = $"Classify \"{request}\" using the following classification table:\n{classes}\n";
        return base.CreateRequestPrompt(fullRequest, preamble);
    }
}
