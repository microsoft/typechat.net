// Copyright (c) Microsoft. All rights reserved.

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

    [JsonIgnore]
    public ReadOnlySpan<float> VectorSpan
    {
        get => _vector.AsSpan();
    }

    /// <summary>
    /// Makes this embedding into a unit vector - in place
    /// If all embeddings have length 1, you can use DotProducts into of full Cosine Similarity. 
    /// </summary>
    public void NormalizeInPlace()
    {
        var length = EuclideanLength(_vector);
        for (int i = 0; i < _vector.Length; ++i)
        {
            _vector[i] = (float)((double) _vector[i] / length);
        }
    }

    /// <summary>
    /// Compute the cosine similarity between this and other
    /// </summary>
    /// <param name="other">other embedding</param>
    /// <returns>cosine similarity</returns>
    public double CosineSimilarity(Embedding other)
    {
        return TensorPrimitives.CosineSimilarity(VectorSpan, other.VectorSpan);
    }

    /// <summary>
    /// The Dot Product of this vector with the
    /// </summary>
    /// <param name="other">other embedding</param>
    /// <returns>dot product</returns>
    public double DotProduct(Embedding other)
    {
        return TensorPrimitives.Dot(VectorSpan, other.VectorSpan);
    }

    /// <summary>
    /// Score the similarity of the two embeddings using the given distance measure
    /// </summary>
    /// <param name="other">embedding to compare to</param>
    /// <param name="type">distance measure type</param>
    /// <returns>score</returns>
    public double Similarity(Embedding other, EmbeddingDistance type)
    {
        if (type == EmbeddingDistance.Dot)
        {
            return TensorPrimitives.Dot(VectorSpan, other.VectorSpan);
        }
        return TensorPrimitives.CosineSimilarity(VectorSpan, other.VectorSpan);
    }

    static double EuclideanLength(float[] vector)
    {
        return Math.Sqrt(TensorPrimitives.Dot(vector, vector));
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
    public bool IsEmpty => _vector is null || _vector.Length == 0;

    public static implicit operator float[](Embedding vector) => vector._vector;

    public static implicit operator Embedding(float[] vector) => new Embedding(vector);

    public static implicit operator Embedding(ReadOnlyMemory<float> src) => new Embedding(src);
}
