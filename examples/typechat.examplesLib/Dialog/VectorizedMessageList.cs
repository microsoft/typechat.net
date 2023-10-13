﻿// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Dialog;

/// <summary>
/// An in-memory stream of messages with a built in in-memory vector index.
/// Messages added to this list are automatically indexed by getting an embedding for their text
/// The index is used to retrieving semantically relevant context for agent interactions
/// </summary>
public class VectorizedMessageList : IMessageStream
{
    public const int DefaultMaxMatches = 10;

    MessageList _messageList;
    // Maps the text of each message to its position in _messageList;
    VectorTextIndex<int> _index;
    int _maxContextMatches;

    /// <summary>
    /// Create a new VectorizedMessageList
    /// </summary>
    /// <param name="embeddingModel">model to use in vectorization</param>
    /// <param name="maxContextMatches">maximum number of semantically relevant matches</param>
    /// <exception cref="ArgumentException"></exception>
    public VectorizedMessageList(TextEmbeddingModel embeddingModel, int maxContextMatches = DefaultMaxMatches)
    {
        ArgumentVerify.ThrowIfNull(embeddingModel, nameof(embeddingModel));
        if (maxContextMatches <= 0)
        {
            throw new ArgumentException(nameof(maxContextMatches));
        }
        _messageList = new MessageList();
        _index = new VectorTextIndex<int>(embeddingModel);
        _maxContextMatches = maxContextMatches;
    }

    /// <summary>
    /// Maximum number of matches to return with GetContextAsync()
    /// </summary>
    public int MaxContextMatches
    {
        get => _maxContextMatches;
        set
        {
            if (value <= 0)
            {
                throw new ArgumentException(nameof(MaxContextMatches));
            }
            _maxContextMatches = value;
        }
    }

    public IEnumerable<Message> All()
    {
        return _messageList.All();
    }

    public IAsyncEnumerable<Message> AllAsync(CancellationToken cancelToken)
    {
        return _messageList.AllAsync(cancelToken);
    }

    public void Append(Message message)
    {
        AppendAsync(message).WaitForResult();
    }

    /// <summary>
    /// Automatically creates an embedding for the message using message.GetText()
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task AppendAsync(Message message)
    {
        int position = _messageList.Count;
        await _messageList.AppendAsync(message).ConfigureAwait(false);
        await _index.AddAsync(position, message.GetText()).ConfigureAwait(false);
    }

    public void Clear()
    {
        _messageList.Clear();
        _index.Items.Clear();
    }

    public void Close()
    {
        _messageList.Close();
        _index.Items.Clear();
    }

    /// <summary>
    /// Return messages that are nearest neighbors of the given request text
    /// </summary>
    /// <param name="request">find messages nearest to this text</param>
    /// <param name="cancelToken">cancel token</param>
    /// <returns></returns>
    public async IAsyncEnumerable<IPromptSection> GetContextAsync(string request, [EnumeratorCancellation] CancellationToken cancelToken = default)
    {
        List<int> matches = await _index.NearestAsync(request, _maxContextMatches, cancelToken).ConfigureAwait(false);
        for (int i = 0; i < matches.Count; ++i)
        {
            yield return _messageList[matches[i]];
        }
    }

    /// <summary>
    /// Return newest messages
    /// </summary>
    /// <returns>an enumerable of messages</returns>
    public IEnumerable<Message> Newest()
    {
        return _messageList.Newest();
    }
    /// <summary>
    /// Return newest messages
    /// </summary>
    /// <returns>an async enumerable of messages</returns>
    public IAsyncEnumerable<Message> NewestAsync(CancellationToken cancelToken)
    {
        return _messageList.NewestAsync(cancelToken);
    }
}
