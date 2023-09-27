// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Dialog;

/// <summary>
/// Agents exchange messages. 
/// </summary>
public class Message : IPromptSection
{
    string _source;
    object _body;

    /// <summary>
    /// Create a new message with the given message body
    /// </summary>
    /// <param name="body">message body</param>
    public Message(object body)
        : this(PromptSection.Sources.User, body)
    {
    }

    /// <summary>
    /// Create a new message
    /// </summary>
    /// <param name="from">source of the message. Common sources include user and assistant</param>
    /// <param name="body">message body</param>
    public Message(string from, object body)
    {
        if (string.IsNullOrEmpty(from))
        {
            from = PromptSection.Sources.User;
        }
        _body = body;
        _source = from;
    }

    /// <summary>
    /// Message source
    /// Common message sources can include "User", "Assistant". See FromUser and FromAssistant methods
    /// </summary>
    public string Source => _source;
    /// <summary>
    /// Message body
    /// </summary>
    public object Body => _body;
    /// <summary>
    /// Message body type
    /// </summary>
    public Type BodyType => _body.GetType();
    /// <summary>
    /// Optional, headers/properties to add to the message. These are useful when messages are routed or persisted
    /// </summary>
    public MessageHeaders? Headers { get; set; }

    /// <summary>
    /// Return the message body serialized as text
    /// You can override this to cache generated text
    /// </summary>
    /// <returns>body as text</returns>
    public virtual string GetText()
    {
        if (_body == null)
        {
            return string.Empty;
        }
        return _body.Stringify();
    }

    public T GetBody<T>() => (T)_body;

    /// <summary>
    /// Create a new message from the given text
    /// </summary>
    /// <param name="text">message body</param>
    public static implicit operator Message(string text) => new Message(text);
    /// <summary>
    /// Create a new message from a user
    /// </summary>
    /// <param name="body">message body</param>
    /// <returns>message</returns>
    public static Message FromUser(object body) => new Message(body);
    /// <summary>
    /// Create a new message from an assistant
    /// </summary>
    /// <param name="body">message body</param>
    /// <returns>message</returns>
    public static Message FromAssistant(object body) => new Message(PromptSection.Sources.Assistant, body);

}

/// <summary>
/// Headers you can optionally add to a message
/// Header names are case-insensitive
/// Headers yinclude timestamps, tags and other information you want to use
/// when searching for contextually relevant messages in a chat. 
/// </summary>
public class MessageHeaders : Dictionary<string, string>
{
    public MessageHeaders()
        : base(StringComparer.OrdinalIgnoreCase)
    {
    }
}
