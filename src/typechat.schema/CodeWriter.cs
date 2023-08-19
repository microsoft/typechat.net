// Copyright (c) Microsoft. All rights reserved.

using System.Text;

namespace Microsoft.TypeChat.Schema;

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
        if (!string.IsNullOrEmpty(token))
        {
            _writer.Write(token);
        }
        return this;
    }

    public CodeWriter Write(StringBuilder sb)
    {
        if (sb.Length > 0)
        {
            _writer.Write(sb);
        }
        return this;
    }

    public CodeWriter Write(double token)
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

    public CodeWriter Append(string token) => Write(token);
    public CodeWriter Append(StringBuilder tokens) => Write(tokens);

    public CodeWriter Append(char ch)
    {
        _writer.Write(ch);
        return this;
    }

    public CodeWriter Clear()
    {
        _indent.Clear();
        return this;
    }

    public CodeWriter LBrace() => Write(CodeLanguage.Punctuation.LBrace);
    public CodeWriter RBrace() => Write(CodeLanguage.Punctuation.RBrace);
    public CodeWriter LParan() => Write(CodeLanguage.Punctuation.LParan);
    public CodeWriter RParan() => Write(CodeLanguage.Punctuation.RParan);
    public CodeWriter Semicolon() => Write(CodeLanguage.Punctuation.Semicolon);
    public CodeWriter Comma() => Write(CodeLanguage.Punctuation.Comma);
    public CodeWriter Period() => Write(CodeLanguage.Punctuation.Period);
    public CodeWriter Colon() => Write(CodeLanguage.Punctuation.Colon);
    public CodeWriter SQuote() => Write(CodeLanguage.Punctuation.SingleQuote);
    public CodeWriter DoubleQuote() => Write(CodeLanguage.Punctuation.DoubleQuote);
    public CodeWriter Question() => Write(CodeLanguage.Punctuation.Question);
    public CodeWriter Space() => Write(Typescript.Punctuation.Space);
    public CodeWriter SOL() { WriteIndent(); return this; } // Start a line
    public CodeWriter EOL() => Write(Typescript.Punctuation.EOL);

    public void Flush()
    {
        _writer.Flush();
    }
}
