// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public class VocabFile
{
    public static VocabCollection Load(string filePath)
    {
        using Stream stream = File.OpenRead(filePath);
        var records = JsonSerializer.Deserialize<Dictionary<string, string[]>>(stream);
        VocabCollection vocabs = new VocabCollection();
        vocabs.Add(records);
        return vocabs;
    }

    public static async Task<VocabCollection> LoadAsync(string filePath)
    {
        using Stream stream = File.OpenRead(filePath);
        return await LoadAsync(stream).ConfigureAwait(false);
    }

    public static async Task<VocabCollection> LoadAsync(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream, nameof(stream));

        var records = await JsonSerializer.DeserializeAsync<Dictionary<string, string[]>>(stream).ConfigureAwait(false);
        VocabCollection vocabs = new VocabCollection();
        vocabs.Add(records);
        return vocabs;
    }
}
