// Copyright (c) Microsoft. All rights reserved.

using System.Reflection;

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

    // *Very* basic checks.
    // Need actual robust validation, e.g. by loading in Typescript
    //   
    public void ValidateBasic(Type type, TypeSchema schema)
    {
        Assert.NotNull(schema);
        Assert.Equal(type, schema.Type);
        Assert.False(string.IsNullOrEmpty(schema.Schema));
    }

    public static void ValidateContains(string text, params string[] values)
    {
        // Kludgy for now
        foreach (var entry in values)
        {
            Assert.Contains(entry, text);
        }
    }

    public bool CanRunEndToEndTest(Config config)
    {
        return (config.HasOpenAI &&
                !string.IsNullOrEmpty(config.OpenAI.ApiKey) &&
                config.OpenAI.ApiKey != "?");
    }

    public MethodInfo? GetMethod(Type type, string name)
    {
        MethodInfo[] methods = type.GetMethods();
        foreach(var method in methods)
        {
            if (method.Name == name)
            {
                return method;
            }
        }
        return null;
    }
}

