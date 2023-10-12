// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Tests;

public class EmojiApi : IAsyncService
{
    public Task DoWork(string userText, Name name)
    {
        return Task.CompletedTask;
    }

    public Task<Name> GetNameOf(string userText, Person person)
    {
        return Task.FromResult(new Name { FirstName = "Mario", LastName = "Minderbinder" });
    }

    public Task<string> Transform(string userText)
    {
        return Task.FromResult("☕📅🍕💊");
    }
}
