// Copyright (c) Microsoft. All rights reserved.

using Calendar;
using HealthData;
using Microsoft.TypeChat.Classification;
using Microsoft.TypeChat.Tests;

namespace Microsoft.TypeChat.UnitTests;

public class TestSchemaFirstExamples
{
    [Fact]
    public async Task ExplicitLegacyProgramPreservesItsFullRequestLayout()
    {
        const string request = "add 3 and 4";
        var model = new RecordingModel("""{"@steps":[{"@func":"add","@args":[3,4]}]}""");
        Api<IMathAPI> api = MathAPI.Default;
        var translator = new ProgramTranslator<IMathAPI>(model, api);
        translator.Prompts = new ProgramTranslatorPrompts(translator.ApiDef);
        using var program = await translator.TranslateAsync(request);

        var call = Assert.Single(model.Calls);
        Assert.Equal(["user", "user"], call.Select(s => s.Role));
        Assert.Contains(translator.Validator.Schema.Schema.Text, call[0].Text);
        Assert.Contains(translator.ApiDef.Text, call[0].Text);
        Assert.Equal(JsonTranslatorPrompts.RequestSection(request + "\n").GetText(), call[1].Text);
    }

    [Fact]
    public async Task MathProgramPutsBothGrammarAndApiInSystem()
    {
        const string request = " \nAdd 3 and 4. ";
        var model = new RecordingModel("""{"@steps":[{"@func":"add","@args":[3,4]}]}""");
        Api<IMathAPI> api = MathAPI.Default;
        var translator = new ProgramTranslator<IMathAPI>(model, api);
        using var program = await translator.TranslateAsync(request);
        Assert.Equal(7d, (double)program.Run(api));

        var call = Assert.Single(model.Calls);
        Assert.Equal(["system", "user"], call.Select(s => s.Role));
        Assert.Contains(translator.Validator.Schema.Schema.Text, call[0].Text);
        Assert.Contains(translator.ApiDef.Text, call[0].Text);
        Assert.Contains("undefined", call[0].Text);
        Assert.DoesNotContain("Add 3 and 4", call[0].Text);
        Assert.Equal(request, call[1].Text);
    }

    [Fact]
    public async Task ClassifierPutsTheCurrentClassTableInSystemNotTheRequest()
    {
        const string request = "  route-only message ";
        var model = new RecordingModel("""{"class":"Calendar"}""");
        var classes = new TextClasses();
        classes.Add("Calendar", "Scheduling-only description");
        var classifier = new TextClassifier(model, classes);
        classes.Add("Shopping", "Shopping-only description");
        Assert.Equal("Calendar", (await classifier.TranslateAsync(request)).Class);

        var call = Assert.Single(model.Calls);
        Assert.Equal(["system", "user"], call.Select(s => s.Role));
        Assert.Contains("Shopping-only description", call[0].Text);
        Assert.Contains("Scheduling-only description", call[0].Text);
        Assert.DoesNotContain("route-only", call[0].Text);
        Assert.Equal(request, call[1].Text);
    }

    [Fact]
    public async Task CalendarSendsClockAsSystemAndUnmodifiedUserText()
    {
        const string request = " tomorrow-only? \n";
        var clock = new FixedClock();
        var model = new RecordingModel("""{"actions":[]}""");
        var app = new CalendarApp(model, clock);
        await app.ProcessInputAsync(request, default);

        var call = Assert.Single(model.Calls);
        Assert.Equal(["system", "system", "user"], call.Select(s => s.Role));
        Assert.Contains(app.Schema.Schema.Text, call[0].Text);
        Assert.Equal(PromptLibrary.Now(clock.GetUtcNow().DateTime).GetText(), call[1].Text);
        Assert.DoesNotContain("tomorrow-only", call[0].Text);
        Assert.Equal(request, call[2].Text);
    }

    [Fact]
    public async Task HealthDataKeepsClockAndInstructionsSeparateFromSelectedHistory()
    {
        const string first = "first-only message";
        const string question = "assistant-only question?";
        const string current = "current-only answer";
        var clock = new FixedClock();
        var model = new RecordingModel($$"""{"Message":"{{question}}"}""", """{"Message":"OK"}""");
        var app = new HealthDataAgent(model, clock);
        await app.ProcessInputAsync(first, default);
        await app.ProcessInputAsync(current, default);

        var call = model.Calls[1];
        Assert.Equal(["system", "system", "system", "system", "user", "assistant", "user"], call.Select(s => s.Role));
        Assert.Contains(app.Schema.Schema.Text, call[0].Text);
        Assert.Equal(PromptLibrary.Now(clock.GetUtcNow().DateTime).GetText(), call[1].Text);
        Assert.Contains("Ask specific questions", call[2].Text);
        Assert.Equal([first, question, current], call.TakeLast(3).Select(s => s.Text));
        foreach (var system in call.Where(s => s.Role == "system"))
        {
            Assert.DoesNotContain(first, system.Text);
            Assert.DoesNotContain(question, system.Text);
            Assert.DoesNotContain(current, system.Text);
        }
    }

    private sealed class FixedClock : TimeProvider
    {
        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
        public override DateTimeOffset GetUtcNow() => new(2030, 2, 3, 14, 15, 16, TimeSpan.Zero);
    }
}
