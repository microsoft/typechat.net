// Copyright (c) Microsoft. All rights reserved.

using System.Diagnostics;
using Microsoft.SemanticKernel;

namespace Microsoft.TypeChat.Tests;

public class TestKernel : TypeChatTest, IClassFixture<Config>
{
    Config _config;

    public TestKernel(Config config)
    {
        _config = config;
    }

    [Fact]
    public void TestKernelBuild()
    {
        if (!_config.HasOpenAI)
        {
            Trace.WriteLine("No OAI. Skipping test");
            return;
        }
        KernelBuilder kb = new KernelBuilder();
        kb.WithChatModels(_config.OpenAI, Config.ModelNames.Gpt35Turbo, Config.ModelNames.Gpt4);
        IKernel kernel = kb.Build();

        var service = kernel.CompletionService(Config.ModelNames.Gpt35Turbo);
        Assert.NotNull(service);
        Assert.Equal(service.Model.Name, Config.ModelNames.Gpt35Turbo);

        service = kernel.CompletionService(Config.ModelNames.Gpt4);
        Assert.NotNull(service);
        Assert.Equal(service.Model.Name, Config.ModelNames.Gpt4);
    }
}
