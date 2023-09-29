// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.TypeChat.Tests;

namespace Microsoft.TypeChat.Tests;

public class EmojiApi : IEmojiService
{
    public Task<string> ToEmoji(string userText)
    {
        return Task.FromResult("☕📅🍕💊");
    }
}
