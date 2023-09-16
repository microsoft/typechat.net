// Copyright (c) Microsoft. All rights reserved.

using System.Diagnostics;

namespace Microsoft.TypeChat.Embeddings;

/// <summary>
/// Embedding are always normalized, i.e. turned into unit vectors
/// This leads to faster cosine simarility as you just need to dotproduts
/// </summary>
public struct Embedding
{
    public static readonly Embedding Empty = new Embedding();

    float[] _vector;

    public Embedding(ReadOnlyMemory<float> vector, bool normalize = true)
    {
        _vector = vector.ToArray();
        if (normalize)
        {
            _vector.NormalizeInPlace(); // Normalize all vectors for faster cosine similarity
        }
    }

    [JsonConstructor]
    public Embedding(float[] vector)
    {
        ArgumentVerify.ThrowIfNull(vector, nameof(vector));
        _vector = vector;
    }

    public double Similarity(Embedding other)
    {
        return _vector.CosineSimilarity(other._vector);
    }

    public float[] Vector
    {
        get => _vector;
        set
        {
            ArgumentVerify.ThrowIfNull(value, nameof(Vector));
            _vector = value;
        }
    }

    [JsonIgnore]
    public bool IsEmpty => _vector == null || _vector.Length == 0;

    public static implicit operator float[](Embedding vector)
    {
        return vector._vector;
    }
    public static implicit operator Embedding(float[] vector)
    {
        return new Embedding(vector);
    }
    public static implicit operator Embedding(ReadOnlyMemory<float> src)
    {
        return new Embedding(src);
    }
}
