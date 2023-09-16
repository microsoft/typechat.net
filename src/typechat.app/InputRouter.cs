// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// Given user input and multiple possible input handlers, the InputRouter 
/// selects the handler semantically most appropriate to handle the input
/// This router uses a Text Classifier. 
/// </summary>
public class InputRouter
{
    TextClassifier _classifier;
    Dictionary<string, IInputHandler> _handlers;

    /// <summary>
    /// Create a new input router
    /// </summary>
    /// <param name="model">model to use for classifying requests</param>
    public InputRouter(ILanguageModel model)
        : this(new TextClassifier(model))
    {
    }

    /// <summary>
    /// Create a new input router
    /// </summary>
    /// <param name="classifier">classifier to use for semantic routing</param>
    public InputRouter(TextClassifier classifier)
    {
        ArgumentNullException.ThrowIfNull(classifier, nameof(classifier));
        _classifier = classifier;
        _handlers = new Dictionary<string, IInputHandler>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Registered routes
    /// </summary>
    public IReadOnlyDictionary<string, IInputHandler> Handlers => _handlers;

    /// <summary>
    /// Add a new handler that user input can be routed to
    /// </summary>
    /// <param name="name">handler name</param>
    /// <param name="handler">handler</param>
    /// <param name="description">A description for the handler: important for good semantic matching</param>
    public void Add(string name, IInputHandler handler, string description)
    {
        _classifier.Classes.Add(name, description);
        _handlers.Add(name, handler);
    }

    /// <summary>
    /// Route the user input to a suitable handler
    /// If the input is NOT semantically related suitable for any registered handler, will return
    /// null as the handler
    /// </summary>
    /// <param name="input">input to route</param>
    /// <param name="cancelToken">optional cancel token</param>
    /// <returns>A key value pair - class name and matching handler</returns>
    public async Task<KeyValuePair<string, IInputHandler?>> RouteAsync(string input, CancellationToken cancelToken = default)
    {
        // Route the user input by classifying it
        TextClassification textClass = await _classifier.TranslateAsync(input, cancelToken);
        // Seleted classes
        string className = textClass.Class != null ? textClass.Class : string.Empty;
        // Now select a handler based on the class
        IInputHandler? selectedHandler = (!string.IsNullOrEmpty(className)) ?
                                        _handlers.GetValueOrDefault(className) :
                                        null;
        // Return the name of the route and the handler - if any
        return new KeyValuePair<string, IInputHandler?>(className, selectedHandler);
    }
}
