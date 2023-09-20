﻿// Copyright (c) Microsoft. All rights reserved.

using System.Diagnostics;

namespace Microsoft.TypeChat.Embeddings;

/// <summary>
/// A Mutable embedding
/// A lightweight struct that wraps an embedding vector
/// </summary>
public struct Embedding
{
    public static readonly Embedding Empty = new Embedding();

    float[] _vector;

    /// <summary>
    /// Create an empty embedding
    /// </summary>
    public Embedding()
    {
        _vector = Empty;
    }
    /// <summary>
    /// Embedding using the given vector. Normalizes the vector before storing it
    /// </summary>
    /// <param name="vector">vector to create embedding from</param>
    public Embedding(ReadOnlyMemory<float> vector)
    {
        _vector = vector.ToArray();
    }

    /// <summary>
    /// Embedding using the given vector. Does not normalize
    /// </summary>
    /// <param name="vector"></param>
    [JsonConstructor]
    public Embedding(float[] vector)
    {
        ArgumentVerify.ThrowIfNull(vector, nameof(vector));
        _vector = vector;
    }

    [JsonIgnore]
    public int Size => _vector.Length;

    /// <summary>
    /// The raw embedding vector
    /// </summary>
    public float[] Vector
    {
        get => _vector;
        set
        {
            ArgumentVerify.ThrowIfNull(value, nameof(Vector));
            _vector = value;
        }
    }

    /// <summary>
    /// Makes this embedding into a unit vector - in place
    /// If all embeddings have length 1, you can use DotProducts into of full Cosine Similarity. 
    /// </summary>
    public void NormalizeInPlace()
    {
        _vector.NormalizeInPlace();
    }
    /// <summary>
    /// Compute the cosine similarity between this and other
    /// </summary>
    /// <param name="other">other embedding</param>
    /// <returns>cosine similarity</returns>
    public double CosineSimilarity(Embedding other)
    {
        return _vector.CosineSimilarity(other._vector);
    }

    /// <summary>
    /// The Dot Product of this vector with the
    /// </summary>
    /// <param name="other">other embedding</param>
    /// <returns>dot product</returns>
    public double DotProduct(Embedding other)
    {
        return _vector.DotProduct(other._vector);
    }

    /// <summary>
    /// Score the similarity of the two embeddings using the given distance measure
    /// </summary>
    /// <param name="other">embedding to compare to</param>
    /// <param name="type">distance measure type</param>
    /// <returns>score</returns>
    public Score Similarity(Embedding other, EmbeddingDistance type)
    {
        if (type == EmbeddingDistance.Dot)
        {
            return _vector.DotProduct(other._vector);
        }
        return _vector.CosineSimilarity(other._vector);
    }

    public bool Equal(Embedding other)
    {
        if (_vector.Length != other._vector.Length)
        {
            return false;
        }
        //
        // This is non-optimal
        //
        for (int i = 0; i < _vector.Length; ++i)
        {
            if (_vector[i] != other._vector[i])
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Is the embedding empty
    /// </summary>
    [JsonIgnore]
    public bool IsEmpty => _vector == null || _vector.Length == 0;

    public static implicit operator float[](Embedding vector) => vector._vector;
    public static implicit operator Embedding(float[] vector) => new Embedding(vector);
    public static implicit operator Embedding(ReadOnlyMemory<float> src) => new Embedding(src);
}
