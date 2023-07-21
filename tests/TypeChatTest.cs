// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Tests;

public class TypeChatTest
{
    public string? GetEnv(string name)
    {
        return Environment.GetEnvironmentVariable(name);
    }

    public string? SetEnv(string name, string value)
    {
        string? prev = GetEnv(name);
        Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.Process);
        return prev;
    }

    public void ClearEnv(string name)
    {
        Environment.SetEnvironmentVariable(name, null, EnvironmentVariableTarget.Process);
    }
}
