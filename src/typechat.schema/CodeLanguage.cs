// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

public class CodeLanguage
{
    public class Punctuation
    {
        public const string LBrace = "{";
        public const string RBrace = "}";
        public const string Colon = ":";
        public const string Question = "?";
        public const string Semicolon = ";";
        public const string Comma = ",";
        public const string Comment = "//";
        public const string Space = " ";
        public const string SingleQuote = "'";
        public const string EOL = "\n";
    }
}

public class CodeWriter
{
    TextWriter _writer;
    List<char> _indent;

    public CodeWriter(TextWriter writer)
    {
        _writer = writer;
        _indent = new List<char>();
    }

    public int IndentUnit { get; set; } = 2;
    public bool IncludeSubclasses { get; set; } = true;

    public CodeWriter PushIndent()
    {
        for (int i = 0; i < IndentUnit; ++i)
        {
            _indent.Add(' ');
        }
        return this;
    }

    public CodeWriter PopIndent()
    {
        if (_indent.Count > 0)
        {
            _indent.RemoveRange(_indent.Count - IndentUnit, IndentUnit);
        }
        return this;
    }

    public CodeWriter Write(string token)
    {
        _writer.Write(token);
        return this;
    }

    public CodeWriter WriteIndent()
    {
        if (_indent.Count > 0)
        {
            for (int i = 0; i < _indent.Count; ++i)
            {
                _writer.Write(_indent[i]);
            }
        }
        return this;
    }

    public CodeWriter Clear()
    {
        _indent.Clear();
        return this;
    }

    public CodeWriter SQuote() => Write(CodeLanguage.Punctuation.SingleQuote);
    public CodeWriter Question() => Write(CodeLanguage.Punctuation.Question);

}
