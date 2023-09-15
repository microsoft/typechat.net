// Copyright (c) Microsoft. All rights reserved.

using Microsoft.VisualBasic;

namespace Microsoft.TypeChat.Dialog;

public class Message : IPromptSection
{
    string _source;
    object _body;

    public Message(object body)
        : this(null, body)
    {
    }

    public Message(string from, object body)
    {
        if (string.IsNullOrEmpty(from))
        {
            from = PromptSection.Sources.User;
        }
        _body = body;
        _source = from;
    }

    public string Source => _source;
    public object Body => _body;
    public Type BodyType => _body.GetType();
    public MessageHeaders? Headers { get; set; }
    public object Attachment { get; set; }

    public T GetBody<T>() => (T) _body;

    public virtual string GetText()
    {
        if (_body == null)
        {
            return string.Empty;
        }
        return _body.Stringify();
    }

    public static implicit operator Message(string text) => new Message(text);
    public static Message FromUser(object body) => new Message(body);
    public static Message FromAssistant(object body) => new Message(PromptSection.Sources.Assistant, body);

}

public class MessageHeaders : Dictionary<string, string>
{
    public MessageHeaders()
        : base(StringComparer.OrdinalIgnoreCase)
    {
    }
}
