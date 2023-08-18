// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

internal class CSharpWriter : CodeWriter
{
    public CSharpWriter(TextWriter writer)
        : base(writer)
    {
    }

    public CSharpWriter Using(string name)
    {
        SOL().Append("using ").Append(name).Semicolon().EOL();
        return this;
    }

    public CSharpWriter Using(IEnumerable<string> names)
    {
        ArgumentNullException.ThrowIfNull(names, nameof(names));
        foreach (string name in names)
        {
            Using(name);
        }
        return this;
    }

    public CSharpWriter Namespace(string name)
    {
        SOL().Append("namespace ").Append(name).Semicolon().EOL();
        return this;
    }

    public CSharpWriter Public()
    {
        Append("public");
        return this;
    }

    public CSharpWriter BeginClass(string name, string? baseName = null)
    {
        SOL();
        Public().Space().Append("class").Space().Append(name);
        if (!string.IsNullOrEmpty(baseName))
        {
            Space().Colon().Space().Append(baseName);
        }
        EOL().LBrace();
        return this;
    }

    public CSharpWriter EndClass()
    {
        SOL().RBrace().EOL();
        return this;
    }
}
