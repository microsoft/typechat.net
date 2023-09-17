// Copyright (c) Microsoft. All rights reserved.

using Microsoft.TypeChat.Embeddings;

namespace Microsoft.TypeChat.Tests;

public class TestVectorized
{
    [Fact]
    public void TestSerialize_Embedding()
    {
        List<Embedding> list = new List<Embedding>();
        for (int i = 0; i < 4; ++i)
        {
            list.Add(Random.Shared.FloatArray(8));
        }
        string json = Json.Stringify(list, false);
        List<Embedding> listBack = Json.Parse<List<Embedding>>(json);

        Assert.Equal(list.Count, listBack.Count);
        for (int i = 0; i < list.Count; ++i)
        {
            ValidateEqual(list[i], listBack[i]);
        }
    }

    [Fact]
    public void TestSerialize_VectorList()
    {
        KeyValuePair<string, Embedding> kv = new KeyValuePair<string, Embedding>("foo", Random.Shared.FloatArray(4));
        string json = Json.Stringify(kv);
        kv = Json.Parse<KeyValuePair<string, Embedding>>(json);

        VectorizedList<string> list = new VectorizedList<string>();
        list.Add("one", Random.Shared.FloatArray(8));
        list.Add("two", Random.Shared.FloatArray(8));

        json = Json.Stringify(list);
        VectorizedList<string> listBack = Json.Parse<VectorizedList<string>>(json);

        Assert.Equal(list.Count, listBack.Count);
    }

    void ValidateEqual(Embedding x, Embedding y)
    {
        Assert.Equal(x.Vector.Length, y.Vector.Length);
        bool cmp = false;
        for (int i = 0; i < x.Vector.Length; ++i)
        {
            cmp = x.Vector[i] == y.Vector[i];
        }
        Assert.True(cmp);
    }
}

