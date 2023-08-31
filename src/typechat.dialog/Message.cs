// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public class Message<T> : IChatMessage
{
    public const string FromUser = "User";

    string _from;
    T _body;

    public Message(T body)
        : this(null, body)
    {
    }

    public Message(string from, T body)
    {
        if (string.IsNullOrEmpty(from))
        {
            from = FromUser;
        }
        _body = body;
        _from = from;
    }

    public string From => _from;

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
        : this(Message.FromUser, body)
    {
    }

    public Message(string source, object body)
        : base(source, body)
    {
    }


    public MessageHeaders? Headers { get; set; }

    public object Attachments { get; set; }

    public static implicit operator Message(string text) => new Message(text);
}
