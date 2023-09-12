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
        if (!CanRunEndToEndTest(_config, nameof(TestKernelBuild)))
        {
            return;
        }
        KernelBuilder kb = new KernelBuilder();
        kb.WithChatModels(_config.OpenAI, Config.ModelNames.Gpt35Turbo, Config.ModelNames.Gpt4);
        IKernel kernel = kb.Build();

        var model = kernel.TextCompletionModel(Config.ModelNames.Gpt35Turbo);
        Assert.NotNull(model);
        Assert.Equal(model.ModelInfo.Name, Config.ModelNames.Gpt35Turbo);

        var languageModel = kernel.LanguageModel(Config.ModelNames.Gpt4);
        Assert.NotNull(languageModel);
        Assert.Equal(languageModel.ModelInfo.Name, Config.ModelNames.Gpt4);
    }

    [Fact]
    public async Task TestLanguageModel()
    {
        if (!CanRunEndToEndTest(_config, nameof(TestLanguageModel)))
        {
            return;
        }

        TextCompletionModel lm = new TextCompletionModel(_config.OpenAI);
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

        LanguageModel cm = new LanguageModel(_config.OpenAI);
        Prompt prompt = "Is Venus a planet?";
        Assert.Equal(prompt.Last().Source, PromptSection.Sources.User);

        string response = await cm.CompleteAsync(prompt);
        Validate(response, "Yes");
        prompt.AppendResponse(response);

        prompt.Append("What about Pluto?");
        response = await cm.CompleteAsync(prompt);
        Validate(response, "No");
        prompt.Append(response);

        Assert.Equal(prompt.Count, 4);
    }

    void Validate(Message message, string? contents = null)
    {
        Assert.NotNull(message.Body);
        if (!string.IsNullOrEmpty(contents))
        {
            Assert.True(message.GetText().Contains(contents, StringComparison.OrdinalIgnoreCase));
        }
    }
}
