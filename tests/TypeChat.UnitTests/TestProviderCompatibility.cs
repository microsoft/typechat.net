// Copyright (c) Microsoft. All rights reserved.

extern alias meai;

using Microsoft.Extensions.AI;
using MeaiLanguageModel = meai::Microsoft.TypeChat.ChatLanguageModel;

namespace Microsoft.TypeChat.Tests;

public class TestProviderCompatibility
{
    private sealed class TestChatClient : IChatClient
    {
        private readonly ChatResponse _response;

        public TestChatClient(ChatResponse response) => _response = response;

        public int Calls { get; private set; }
        public ChatOptions LastOptions { get; private set; }
        public CancellationToken LastToken { get; private set; }

        public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
        {
            Calls++;
            LastOptions = options;
            LastToken = cancellationToken;
            return Task.FromResult(_response);
        }

        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("These tests do not use streaming.");

        public object? GetService(Type serviceType, object? serviceKey = null) => null;

        public void Dispose() { }
    }

    [Theory]
    [InlineData(typeof(LanguageModel))]
    [InlineData(typeof(ChatLanguageModel))]
    [InlineData(typeof(TextCompletionModel))]
    [InlineData(typeof(MeaiLanguageModel))]
    public void ProvidersPreserveTheOriginalCompletionSignature(Type provider)
    {
        Assert.Equal(typeof(Task<string>), provider.GetMethod(nameof(ILanguageModel.CompleteAsync)).ReturnType);
        Assert.True(typeof(ILanguageModel).IsAssignableFrom(provider));
    }

    [Theory]
    [InlineData("stop", CompletionFinishReason.Stop)]
    [InlineData("length", CompletionFinishReason.Length)]
    [InlineData("content_filter", CompletionFinishReason.ContentFilter)]
    [InlineData("tool_calls", CompletionFinishReason.ToolCalls)]
    [InlineData("custom", CompletionFinishReason.Other)]
    public async Task ExtensionsAiProviderExposesMetadataWithoutChangingTextCompletions(string finishReason, CompletionFinishReason expectedReason)
    {
        using var client = new TestChatClient(new ChatResponse(new ChatMessage(ChatRole.Assistant, "hello"))
        {
            ModelId = "reported-model",
            FinishReason = new ChatFinishReason(finishReason),
            Usage = new UsageDetails { InputTokenCount = 9, OutputTokenCount = 12, TotalTokenCount = 21 }
        });
        var model = new MeaiLanguageModel(client, "requested-model");
        var settings = new TranslationSettings { Temperature = 0.5, MaxTokens = 32 };
        using var cancellation = new CancellationTokenSource();

        Task<string> originalCall = model.CompleteAsync("request", settings, cancellation.Token);
        Assert.Equal("hello", await originalCall);
        Assert.Equal(1, client.Calls);
        ILanguageModel originalInterface = model;
        var response = await originalInterface.CompleteWithMetadataAsync("request", settings, cancellation.Token);

        Assert.Equal("hello", response.Text);
        Assert.Equal("reported-model", response.Info.Model);
        Assert.Equal(expectedReason, response.Info.FinishReason);
        Assert.Equal(9, response.Info.Usage.PromptTokens);
        Assert.Equal(12, response.Info.Usage.CompletionTokens);
        Assert.Equal(21, response.Info.Usage.TotalTokens);
        Assert.Equal(0.5f, client.LastOptions.Temperature);
        Assert.Equal(32, client.LastOptions.MaxOutputTokens);
        Assert.Equal(cancellation.Token, client.LastToken);
        Assert.Equal(2, client.Calls);
    }
}
