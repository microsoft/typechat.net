// Copyright (c) Microsoft. All rights reserved.

using System.Reflection;

namespace Microsoft.TypeChat.Schema;

[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
public class CommentAttribute : Attribute
{
    public CommentAttribute() { }

    public CommentAttribute(string text)
    {
        Text = text;
    }

    public string? Text { get; set; }

    public bool HasText => !string.IsNullOrEmpty(Text);
}
