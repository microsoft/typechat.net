// Copyright (c) Microsoft. All rights reserved.

using Microsoft.TypeChat.Tests;

namespace Microsoft.TypeChat.UnitTests;

public class TestLanguageModel : TypeChatTest
{
    [Fact]
    public async Task TestRetry()
    {
        var handler = MockHttpHandler.ErrorResponder(429);
        var config = MockOpenAIConfig();
        config.MaxRetries = 2;
        config.MaxPauseMs = 0;

        LanguageModel model = new LanguageModel(config, null, new HttpClient(handler));
        await Assert.ThrowsAnyAsync<Exception>(() => model.CompleteAsync("Hello"));
        Assert.Equal(config.MaxRetries + 1, handler.RequestCount);
    }
}
