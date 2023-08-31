// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public interface IMessage
{
    string From { get; }
    string GetText();
}

public class Message<T> : IMessage
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

    public virtual string GetText()
    {
        if (_body == null)
        {
            return string.Empty;
        }
        return _body.Stringify();
    }
}

public class Message : Message<object>
{
    public Message(object body, string? from = null)
        : base(body, from)
    {
    }

    public static implicit operator Message(string text) => new Message(text);
}
