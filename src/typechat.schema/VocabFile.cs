// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

/// <summary>
/// Helper methods for loading vocabularies from a Json files and streams
/// The file is a dictionary of json properties like this:
/// {
/// "coffeeDrink": ["americano", "coffee"],
/// "toppings": ["cinnamon", "almond", "vanilla"]
/// }
/// </summary>
public static class VocabFile
{
    /// <summary>
    /// Load a vocabulary from a JSON file
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static VocabCollection Load(string filePath)
    {
        using Stream stream = File.OpenRead(filePath);
        var records = JsonSerializer.Deserialize<Dictionary<string, string[]>>(stream);
        return new VocabCollection(records);
    }

    /// <summary>
    /// Load a vocabulary from a json file
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static async Task<VocabCollection> LoadAsync(string filePath)
    {
        using Stream stream = File.OpenRead(filePath);
        return await LoadAsync(stream).ConfigureAwait(false);
    }

    /// <summary>
    /// Load a vocabulary from the given stream
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static async Task<VocabCollection> LoadAsync(Stream stream)
    {
        ArgumentVerify.ThrowIfNull(stream, nameof(stream));

        var records = await JsonSerializer.DeserializeAsync<Dictionary<string, string[]>>(stream).ConfigureAwait(false);
        return new VocabCollection(records);
    }
}
