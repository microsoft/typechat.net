// Copyright (c) Microsoft. All rights reserved.

using System.Text.Json.Serialization;
using Microsoft.TypeChat.Dialog;

namespace Microsoft.TypeChat.UnitTests;

public class TestSchemaFirstPrompts
{
    public class CountResponse
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }
    }

    private const string Valid = """{"count":3}""";
    private const string Invalid = " \n{ \"count\": \"not-a-number\" }\t\n";
    private const string Current = " \tcurrent-only ☕\r\n\"\"\"\n###\n ";

    [Fact]
    public async Task FreshRequestIsOneVerbatimUserTurnAfterSystemSchema()
    {
        var model = new RecordingModel(Valid);
        var translator = new JsonTranslator<CountResponse>(model);
        Assert.Equal(3, (await translator.TranslateAsync(Current)).Count);

        var call = Assert.Single(model.Calls);
        Assert.Equal(["system", "user"], call.Select(s => s.Role));
        Assert.Contains(translator.Validator.Schema.Schema.Text, call[0].Text);
        Assert.Contains("2 spaces", call[0].Text);
        Assert.Contains("undefined", call[0].Text);
        Assert.DoesNotContain("current-only", call[0].Text);
        Assert.Equal(Current, call[1].Text);
    }

    [Fact]
    public async Task PreambleAndHistoryKeepCallerRolesAndOrderOutsideSystem()
    {
        var model = new RecordingModel(Valid);
        var translator = new JsonTranslator<CountResponse>(model);
        Prompt preamble = new()
        {
            PromptSection.FromSystem("app-owned preamble"),
            PromptSection.FromUser("prior-only question"),
            PromptSection.FromAssistant("prior-only raw answer\n"),
            PromptSection.FromUser("retrieved-only context")
        };
        await translator.TranslateAsync(Current, preamble);

        var call = Assert.Single(model.Calls);
        Assert.Equal(["system", "system", "user", "assistant", "user", "user"], call.Select(s => s.Role));
        Assert.Equal(preamble.ToArray(), call.Skip(1).SkipLast(1).Select(s => s.Original));
        Assert.Equal(Current, call[^1].Text);
        foreach (var system in call.Where(s => s.Role == "system"))
        {
            Assert.DoesNotContain("prior-only", system.Text);
            Assert.DoesNotContain("retrieved-only", system.Text);
            Assert.DoesNotContain("current-only", system.Text);
        }
    }

    [Fact]
    public async Task StructuredRequestRetainsEveryNativeSectionByIdentity()
    {
        var model = new RecordingModel(Valid);
        var translator = new JsonTranslator<CountResponse>(model);
        var body = Message.FromUser(new { note = "structured-only" });
        Prompt request = new() { new PromptSection("tool", "earlier tool result"), body };
        await translator.TranslateAsync(request, null);

        var call = Assert.Single(model.Calls);
        Assert.Equal(["system", "tool", "user"], call.Select(s => s.Role));
        Assert.Same(request[0], call[1].Original);
        Assert.Same(body, call[2].Original);
        Assert.DoesNotContain("structured-only", call[0].Text);
        Assert.Equal(2, request.Count);
    }

    [Fact]
    public async Task ParseableSchemaErrorRepairsWithRawAssistantAndUserDiagnostic()
    {
        var model = new RecordingModel(Invalid, Valid);
        var translator = new JsonTranslator<CountResponse>(model);
        string? diagnostic = null;
        translator.AttemptingRepair += message => diagnostic = message;
        Assert.Equal(3, (await translator.TranslateAsync(Current)).Count);

        Assert.Equal(2, model.Calls.Count);
        var repair = model.Calls[1];
        Assert.Equal(["system", "user", "assistant", "user"], repair.Select(s => s.Role));
        Assert.Equal(model.Calls[0], repair.Take(2));
        Assert.Equal(Current, repair[1].Text);
        Assert.Equal(Invalid, repair[2].Text);
        Assert.False(string.IsNullOrEmpty(diagnostic));
        Assert.Contains(diagnostic, repair[3].Text);
        Assert.DoesNotContain("not-a-number", repair[0].Text);
        Assert.DoesNotContain(diagnostic, repair[0].Text);
    }

    [Fact]
    public async Task ExplicitLegacyUsesTheWrappedUserRequest()
    {
        var model = new RecordingModel(Valid);
        var translator = new JsonTranslator<CountResponse>(model, JsonTranslatorPrompts.Legacy);
        await translator.TranslateAsync(Current);

        var call = Assert.Single(model.Calls);
        Assert.Equal(["user", "user"], call.Select(s => s.Role));
        Assert.Contains(translator.Validator.Schema.Schema.Text, call[0].Text);
        Assert.Equal(JsonTranslatorPrompts.RequestSection(Current).GetText(), call[1].Text);
    }

    [Fact]
    public async Task LegacyWrapsStructuredRequestsWithoutDroppingTheirSections()
    {
        var model = new RecordingModel(Valid);
        var translator = new JsonTranslator<CountResponse>(model, JsonTranslatorPrompts.Legacy);
        var body = Message.FromUser(new { note = "structured-only" });
        Prompt request = new() { PromptSection.FromAssistant("supplied answer"), body };
        await translator.TranslateAsync(request, null);

        var call = Assert.Single(model.Calls);
        Assert.Equal(["user", "system", "assistant", "user", "user"], call.Select(s => s.Role));
        Assert.Same(request[0], call[2].Original);
        Assert.Same(body, call[3].Original);
        Assert.DoesNotContain("structured-only", call[1].Text);
    }

    [Fact]
    public async Task CustomFullRequestAndRepairHooksRunWithoutRewritingOrPromotion()
    {
        var model = new RecordingModel(Invalid, Valid);
        var custom = new FullRequestPrompts();
        var translator = new HookedTranslator(model) { Prompts = custom };
        Prompt request = Current;
        Prompt preamble = new() { PromptSection.FromAssistant("caller history") };
        await translator.TranslateAsync(request, preamble);

        Assert.Equal(1, translator.BuiltRequests);
        Assert.Same(request, custom.Request);
        Assert.Same(preamble, custom.Preamble);
        Assert.Equal(Invalid, custom.Rejected);
        Assert.Equal(["assistant", "user"], model.Calls[0].Select(s => s.Role));
        Assert.Equal(["caller history", "custom: " + Current], model.Calls[0].Select(s => s.Text));
        Assert.Equal(["assistant", "user", "assistant", "user"], model.Calls[1].Select(s => s.Role));
        Assert.StartsWith("custom repair: ", model.Calls[1][^1].Text);
    }

    private sealed class HookedTranslator(ILanguageModel model) : JsonTranslator<CountResponse>(model)
    {
        public int BuiltRequests { get; private set; }

        protected override Prompt CreateRequestPrompt(Prompt request, IList<IPromptSection> preamble)
        {
            BuiltRequests++;
            return base.CreateRequestPrompt(request, preamble);
        }
    }

    private sealed class FullRequestPrompts : IJsonTranslatorPrompts
    {
        public Prompt? Request { get; private set; }
        public IList<IPromptSection>? Preamble { get; private set; }
        public string? Rejected { get; private set; }

        public Prompt CreateRequestPrompt(TypeSchema schema, Prompt request, IList<IPromptSection> preamble)
        {
            Request = request;
            Preamble = preamble;
            return new Prompt(preamble, PromptSection.FromUser("custom: " + request[0].GetText()));
        }

        public string CreateRepairPrompt(TypeSchema schema, string json, string validationError)
        {
            Rejected = json;
            return "custom repair: " + validationError;
        }
    }
}
