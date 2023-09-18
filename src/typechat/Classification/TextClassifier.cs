// Copyright (c) Microsoft. All rights reserved.

using Microsoft.TypeChat.Schema;

namespace Microsoft.TypeChat.Classification;

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

