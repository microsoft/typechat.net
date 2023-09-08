// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Dialog;

public class Message<T> : IPromptSection
{
    string _source;
    T _body;

    public Message(T body)
        : this(null, body)
    {
    }

    public Message(string from, T body)
    {
        if (string.IsNullOrEmpty(from))
        {
            from = PromptSection.Sources.User;
        }
        _body = body;
        _source = from;
    }

    public string Source => _source;

    public T Body => _body;

    public Type BodyType => _body.GetType();

    public virtual string GetText()
    {
        if (_body == null)
        {
            return string.Empty;
        }
        return _body.Stringify();
    }
}

public class MessageHeaders : Dictionary<string, string>
{
    public MessageHeaders()
        : base(StringComparer.OrdinalIgnoreCase)
    {
    }
}

/// <summary>
/// General message
/// </summary>
public class Message : Message<object>
{
    public Message(object body)
        : base(body)
    {
    }

    public Message(string source, object body)
        : base(source, body)
    {
    }


    public MessageHeaders? Headers { get; set; }

    public object Attachment { get; set; }

    public static implicit operator Message(string text) => new Message(text);

    public static Message FromUser(object body) => new Message(body);
    public static Message FromAssistant(object body) => new Message(PromptSection.Sources.Assistant, body);
}
