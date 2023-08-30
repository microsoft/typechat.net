// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public class Message<T>
{
    public const string DefaultFrom = "User";

    string _from;
    T _body;

    [JsonConstructor]
    public Message(T body, string? from = null)
    {
        if (string.IsNullOrEmpty(from))
        {
            from = DefaultFrom;
        }
        _body = body;
        _from = from;
    }

    [JsonPropertyName("from")]
    public string From => _from;

    [JsonPropertyName("body")]
    public T Body => _body;

    [JsonIgnore]
    public Type BodyType => _body.GetType();

    public virtual string BodyAsText()
    {
        if (_body == null)
        {
            return string.Empty;
        }
        return _body.Stringify();
    }
}

public class Message : Message<string>
{
    public Message(string body, string? from = null)
        : base(body, from)
    {
    }

    public override string BodyAsText()
    {
        return Body;
    }

    public static implicit operator string(Message message) => message.Body;
    public static implicit operator Message(string text) => new Message(text);
}
