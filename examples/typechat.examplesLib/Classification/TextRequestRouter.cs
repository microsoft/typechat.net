// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Classification;

/// <summary>
/// Uses a TextClassifier to route requests to the most semantically related targets
/// </summary>
/// <typeparam name="T"></typeparam>
public class TextRequestRouter<T> : ITextRequestRouter<T>
{
    private TextClassifier _classifier;
    private Dictionary<string, T> _routes;

    /// <summary>
    /// Create a router that creates a classifier that uses the given language model
    /// </summary>
    /// <param name="model">model to use for classifying requests</param>
    public TextRequestRouter(ILanguageModel model)
        : this(new TextClassifier(model))
    {
    }

    /// <summary>
    /// Create a router that uses the given TextClassifier
    /// </summary>
    /// <param name="classifier">classifier to use for semantic routing</param>
    public TextRequestRouter(TextClassifier classifier)
    {
        ArgumentVerify.ThrowIfNull(classifier, nameof(classifier));
        _classifier = classifier;
        _routes = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Registered routes
    /// </summary>
    public IReadOnlyDictionary<string, T> Routes => _routes;

    /// <summary>
    /// Add a route
    /// : nameOfTarget should be unique and typically short
    /// : description describes what kind of natural requests should be routed to this target
    /// </summary>
    /// <param name="target">potential target for requests. Can be null for ignore routes</param>
    /// <param name="nameOfTarget">short, unique name of the target</param>
    /// <param name="description">description of the target</param>
    public void Add(T target, string nameOfTarget, string description)
    {
        ArgumentVerify.ThrowIfNullOrEmpty(nameOfTarget, nameof(nameOfTarget));
        ArgumentVerify.ThrowIfNullOrEmpty(description, nameof(description));

        _classifier.Classes.Add(nameOfTarget, description);
        _routes.Add(nameOfTarget, target);
    }

    /// <summary>
    /// Route the given request
    /// </summary>
    /// <param name="request">natural language request to route</param>
    /// <param name="cancellationToken">optional cancel token</param>
    /// <returns></returns>
    public async Task<T> RouteRequestAsync(string request, CancellationToken cancellationToken = default)
    {
        // Route the user input by using the language model to classify it
        TextClassification classification = await _classifier.TranslateAsync(request, cancellationToken).ConfigureAwait(false);
        if (classification.HasClass && _routes.TryGetValue(classification.Class, out T target))
        {
            return target;
        }

        return default;
    }

    /// <summary>
    /// Classify the given request
    /// </summary>
    /// <param name="request">request</param>
    /// <param name="cancellationToken">optional cancel token</param>
    /// <returns>The class and route</returns>
    public async Task<KeyValuePair<string, T>> ClassifyRequestAsync(string request, CancellationToken cancellationToken = default)
    {
        TextClassification classification = await _classifier.TranslateAsync(request, cancellationToken).ConfigureAwait(false);
        if (classification.HasClass && _routes.TryGetValue(classification.Class, out T target))
        {
            return new KeyValuePair<string, T>(classification.Class, target);
        }

        return default;
    }
}
