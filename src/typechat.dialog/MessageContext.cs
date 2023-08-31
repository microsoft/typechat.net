// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public class MessageContext
{
    MessageList _messages;
    int _currentLength;
    int _maxLength;
    bool _allowSubstrings;

    public MessageContext(int maxLength, bool allowSubstrings = false)
    {
        _maxLength = maxLength;
        _messages = new MessageList();
        _allowSubstrings = allowSubstrings;
    }

    public int Count => _messages.Count;
    public MessageList Messages => _messages;

    public void BeginContext()
    {
        Clear();
    }

    public bool Append(Message message)
    {
        ArgumentNullException.ThrowIfNull(message, nameof(message));

        string? text = message.GetText();
        if (text != null && (_currentLength + text.Length) <= _maxLength)
        {
            _messages.Add(message);
            _currentLength += text.Length;
            return true;
        }
        else if (_allowSubstrings)
        {
            // Always append the last one, regardless of length.
            // ToString will do the right thing..
            _messages.Append(message);
        }
        return false;
    }

    public int Append(IEnumerable<Message> messages)
    {
        ArgumentNullException.ThrowIfNull(messages, nameof(messages));
        int count = 0;
        foreach (var msg in messages)
        {
            if (!Append(msg))
            {
                break;
            }
            ++count;
        }
        return count;
    }

    public bool Append(string message, string? source = null)
    {
        source ??= Message.FromUser;
        if (!string.IsNullOrEmpty(message))
        {
            return Append(new Message(source, message));
        }
        return true;
    }

    public bool Append(IEnumerable<string> messages, string? source = null)
    {
        foreach (var message in messages)
        {
            if (!Append(message, source))
            {
                return false;
            }
        }
        return true;
    }

    public void Reverse()
    {
        Reverse(0, _messages.Count);
    }

    public void Reverse(int startAt, int count)
    {
        _messages.Reverse(startAt, count);
    }

    public void Clear()
    {
        _messages.Clear();
        _currentLength = 0;
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder(_currentLength);
        if (_allowSubstrings)
        {
            AppendToWithSubstring(sb);
        }
        else
        {
            AppendTo(sb);
        }
        return sb.ToString();
    }

    public StringBuilder AppendTo(StringBuilder sb)
    {
        sb ??= new StringBuilder();
        foreach (var message in _messages)
        {
            if (sb.Length >= _maxLength)
            {
                break;
            }
            var messageText = message.GetText() + '\n';
            if (messageText.Length + sb.Length >= _maxLength)
            {
                break;
            }
            sb.Append(messageText);
        }
        return sb;
    }

    public StringBuilder AppendToWithSubstring(StringBuilder sb)
    {
        sb ??= new StringBuilder();
        foreach (var message in _messages)
        {
            int availableLength = (_maxLength - sb.Length);
            if (availableLength <= 0)
            {
                break;
            }
            string messageText = message.GetText() + '\n';
            if (messageText.Length > availableLength)
            {
                sb.Append(messageText, 0, availableLength);
                break;
            }
            else
            {
                sb.Append(messageText);
            }
        }
        return sb;
    }
}
