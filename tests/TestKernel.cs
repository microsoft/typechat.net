// Copyright (c) Microsoft. All rights reserved.

using System.Diagnostics;

using Microsoft.SemanticKernel.AI.ChatCompletion;

namespace Microsoft.TypeChat.Tests;

public class TestKernel : TypeChatTest, IClassFixture<Config>
{
    Config _config;

    public TestKernel(ITestOutputHelper output, Config config)
        : base(output)
    {
        _config = config;
    }

    [Fact]
    public void TestKernelBuild()
    {
        if (!_config.HasOpenAI)
        {
            WriteSkipped(nameof(TestKernelBuild), "No OpenAI Config");
            return;
        }
        KernelBuilder kb = new KernelBuilder();
        kb.WithChatModels(_config.OpenAI, Config.ModelNames.Gpt35Turbo, Config.ModelNames.Gpt4);
        IKernel kernel = kb.Build();

        var service = kernel.LanguageModel(Config.ModelNames.Gpt35Turbo);
        Assert.NotNull(service);
        Assert.Equal(service.Model.Name, Config.ModelNames.Gpt35Turbo);

        service = kernel.LanguageModel(Config.ModelNames.Gpt4);
        Assert.NotNull(service);
        Assert.Equal(service.Model.Name, Config.ModelNames.Gpt4);
    }

    [Fact]
    public async Task TestLanguageModel()
    {
        if (!CanRunEndToEndTest(_config, nameof(TestLanguageModel)))
        {
            return;
        }

        LanguageModel lm = new LanguageModel(_config.OpenAI);
        string response = await lm.CompleteAsync("Is Venus a planet?");
        Assert.NotNull(response);
        Assert.NotEmpty(response);
    }

    [Fact]
    public async Task TestChatModel()
    {
        if (!CanRunEndToEndTest(_config, nameof(TestLanguageModel)))
        {
            return;
        }

        List<Message> history = new List<Message>();
        ChatModel cm = new ChatModel(_config.OpenAI);

        Message userMessage = "Is Venus a planet?";
        Assert.Equal(userMessage.GetRole(), AuthorRole.User);

        Message response = await cm.GetResponseAsync(userMessage);
        Validate(response, AuthorRole.Assistant, "Yes");
        history.Add(userMessage);
        history.Add(response);

        userMessage = "What about Pluto?";
        response = await cm.GetResponseAsync(userMessage, history);
        Validate(response, AuthorRole.Assistant, "No");
        history.Add(userMessage);
        history.Add(response);

        Assert.Equal(history.Count, 4);
    }

    void Validate(Message message, AuthorRole role, string? contents = null)
    {
        Assert.False(string.IsNullOrEmpty(message.Body));
        Assert.Equal(message.From, role.Label);
        if (!string.IsNullOrEmpty(contents))
        {
            Assert.True(message.Body.Contains(contents, StringComparison.OrdinalIgnoreCase));
        }
    }
}
