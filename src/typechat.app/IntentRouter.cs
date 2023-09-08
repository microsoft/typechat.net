﻿// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public class IntentRouter<T>
{
    ILanguageModel _model;
    TextClassifier _classifier;
    Dictionary<string, T> _routes;

    public IntentRouter(ILanguageModel model)
        : this(new TextClassifier(model))
    {
    }

    public IntentRouter(TextClassifier classifier)
    {
        ArgumentNullException.ThrowIfNull(classifier, nameof(classifier));
        _classifier = classifier;
        _routes = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
    }

    public Dictionary<string, T> Routes => _routes;

    public void Add(string key, T value, string description)
    {
        _classifier.Classes.Add(key, description);
        _routes.Add(key, value);
    }

    public async Task<KeyValuePair<string, T?>> RouteAsync(string intent, CancellationToken cancelToken = default)
    {
        TextClassification selectedRoute = await _classifier.TranslateAsync(intent, cancelToken);
        // Route input 
        T? route = (selectedRoute.Class != null) ? _routes.GetValueOrDefault(selectedRoute.Class) : default;
        return new KeyValuePair<string, T?>(selectedRoute.Class, route);
    }
}
