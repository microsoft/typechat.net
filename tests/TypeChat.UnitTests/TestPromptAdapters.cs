// Copyright (c) Microsoft. All rights reserved.

extern alias ExtensionsAI;

using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.TextGeneration;
using MeaiChatModel = ExtensionsAI::Microsoft.TypeChat.ChatLanguageModel;
using TextContent = Microsoft.SemanticKernel.TextContent;

namespace Microsoft.TypeChat.UnitTests;

public class TestPromptAdapters
{
    private const string Valid = """{"count":3}""";

    [Fact]
    public async Task TextCompletionRefusesRoleLossBeforeCallingTheService()
    {
        var service = new RecordingTextService();
        var model = new TextCompletionModel(service, "recorder");
        var translator = new JsonTranslator<TestSchemaFirstPrompts.CountResponse>(model);
        var error = await Assert.ThrowsAsync<NotSupportedException>(() => translator.TranslateAsync("current"));
        Assert.Contains("cannot preserve chat roles", error.Message);
        Assert.Empty(service.Requests);

        translator.Prompts = JsonTranslatorPrompts.Legacy;
        Assert.Equal(3, (await translator.TranslateAsync("current")).Count);
        Assert.Contains(JsonTranslatorPrompts.RequestSection("current").GetText(), Assert.Single(service.Requests));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ChatAdaptersForwardCallerRolesAndMessages(bool useExtensionsAI)
    {
        var history = new List<(string Role, string Text)>();
        ILanguageModel model = useExtensionsAI ?
            new MeaiChatModel(new RecordingChatClient(history), "recorder") :
            new ChatLanguageModel(new RecordingChatService(history), "recorder");
        var translator = new JsonTranslator<TestSchemaFirstPrompts.CountResponse>(model);
        Prompt preamble = new() { new PromptSection("tool", "tool-only result") };
        await translator.TranslateAsync("current-only", preamble);

        Assert.Equal(["system", "tool", "user"], history.Select(m => m.Role));
        Assert.DoesNotContain("tool-only", history[0].Text);
        Assert.DoesNotContain("current-only", history[0].Text);
        Assert.Equal("tool-only result", history[1].Text);
        Assert.Equal("current-only", history[2].Text);
    }

    private sealed class RecordingTextService : ITextGenerationService
    {
        public IReadOnlyDictionary<string, object?> Attributes { get; } = new Dictionary<string, object?>();
        public List<string> Requests { get; } = [];

        public Task<IReadOnlyList<TextContent>> GetTextContentsAsync(string prompt,
            PromptExecutionSettings? executionSettings, Kernel? kernel, CancellationToken cancellationToken)
        {
            Requests.Add(prompt);
            return Task.FromResult<IReadOnlyList<TextContent>>([new TextContent(Valid)]);
        }

        public IAsyncEnumerable<StreamingTextContent> GetStreamingTextContentsAsync(string prompt,
            PromptExecutionSettings? executionSettings, Kernel? kernel, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }

    private sealed class RecordingChatService(List<(string, string)> history) : IChatCompletionService
    {
        public IReadOnlyDictionary<string, object?> Attributes { get; } = new Dictionary<string, object?>();

        public Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(ChatHistory chatHistory,
            PromptExecutionSettings? executionSettings, Kernel? kernel, CancellationToken cancellationToken)
        {
            history.AddRange(chatHistory.Select(m => (m.Role.Label, m.Content)));
            return Task.FromResult<IReadOnlyList<ChatMessageContent>>([new ChatMessageContent(AuthorRole.Assistant, Valid)]);
        }

        public IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(ChatHistory chatHistory,
            PromptExecutionSettings? executionSettings, Kernel? kernel, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }

    private sealed class RecordingChatClient(List<(string, string)> history) : IChatClient
    {
        public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options, CancellationToken cancellationToken)
        {
            history.AddRange(messages.Select(m => (m.Role.Value, m.Text)));
            return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, Valid)));
        }

        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages,
            ChatOptions? options, CancellationToken cancellationToken) => throw new NotSupportedException();

        public object? GetService(Type serviceType, object? serviceKey) => null;
        public void Dispose() { }
    }
}
