// Copyright (c) Microsoft. All rights reserved.

using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.TypeChat.Tests;

public class TestFailureContext
{
    public class Example
    {
        public int Value { get; set; }
    }

    private sealed class FixedModel : ILanguageModel
    {
        private readonly Queue<object> _responses;

        public FixedModel(params object[] responses)
        {
            _responses = new Queue<object>(responses);
        }

        public ModelInfo ModelInfo => "Mock";

        public List<string[]> Prompts { get; } = new List<string[]>();

        public Task<string> CompleteAsync(Prompt prompt, TranslationSettings? settings, CancellationToken cancelToken)
        {
            Prompts.Add(prompt.Select(section => section.GetText()).ToArray());
            object response = _responses.Dequeue();
            return response is Exception error ?
                Task.FromException<string>(error) :
                Task.FromResult((string)response);
        }
    }

    private sealed class FixedValidator : IJsonTypeValidator<Example>
    {
        private readonly Result<Example> _result;

        public FixedValidator(Result<Example> result)
        {
            _result = result;
        }

        public TypeSchema Schema { get; } = ExampleSchema();

        public Result<Example> Validate(string json) => _result;
    }

    private sealed class FixedConstraints : IConstraintsValidator<Example>
    {
        private readonly Result<Example> _result;

        public FixedConstraints(Result<Example> result)
        {
            _result = result;
        }

        public Result<Example> Validate(Example value) => _result;
    }

    private sealed class CustomFailureInfo : FailureInfo
    {
        public string Code { get; set; } = "CUSTOM";
    }

    private sealed class CustomResult : Result<Example>
    {
        public CustomResult()
            : base(Result<object?>.Error("custom"))
        {
        }

        public string Code { get; } = "CUSTOM";
    }

    private static JsonTranslator<Example> Translator(FixedModel model, int repairs = 1)
    {
        return new JsonTranslator<Example>(model, new FixedValidator(Result<Example>.Error("invalid value")))
        {
            MaxRepairAttempts = repairs
        };
    }

    private static TypeSchema ExampleSchema()
    {
        return new TypeSchema(typeof(Example),
            new SchemaText("export interface Example { Value: number; }", SchemaText.Languages.Typescript));
    }

    [Fact]
    public void MessageOnlyErrorsHaveNoFailureContext()
    {
        Result<int> result = Result<int>.Error("failed");
        Assert.False(result.Success);
        Assert.Equal(0, result.Value);
        Assert.Null(result.Failure);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(false)]
    [InlineData(0)]
    [InlineData("")]
    public void ExplicitFailureValuesAreRetained(object? value)
    {
        Result<object?> result = Result<object?>.Error(value, "failed");
        Assert.False(result.Success);
        Assert.Same(value, result.Value);
        Assert.True(result.Failure.HasValue);
        Assert.Same(value, result.Failure.Value);
    }

    [Fact]
    public void CloningFailuresDoesNotCastRejectedDataToTheSuccessType()
    {
        var source = Result<object?>.Error("rejected", "failed");
        var clone = new Result<int>(source);
        Assert.False(clone.Success);
        Assert.Equal(0, clone.Value);
        Assert.Equal("rejected", clone.Failure.Value);
    }

    [Fact]
    public void AddingContextDoesNotMutateExistingFailures()
    {
        Result<Example> original = Result<Example>.Error("failed");
        Result<Example> enriched = original.WithFailure(new FailureInfo { Value = null, ResponseText = "" });
        Assert.Null(original.Failure);
        Assert.True(enriched.Failure.HasValue);
        Assert.Null(enriched.Failure.Value);
        Assert.Equal("", enriched.Failure.ResponseText);
    }

    [Fact]
    public void AddingContextPreservesCustomResultSubclasses()
    {
        var original = new CustomResult();
        var enriched = Assert.IsType<CustomResult>(original.WithFailure(new FailureInfo { Value = 0 }));
        Assert.Equal("CUSTOM", enriched.Code);
        Assert.Equal(0, enriched.Failure.Value);
        Assert.Null(original.Failure);
    }

    [Fact]
    public void ExistingFailureMetadataTakesPrecedenceOverFallbacks()
    {
        var metadata = new CustomFailureInfo { Value = null, ResponseText = "" };
        Result<Example> original = Result<Example>.Error("custom").WithFailure(metadata);
        Result<Example> enriched = original.WithFailure(new FailureInfo { Value = 42, ResponseText = "fallback" });
        var failure = Assert.IsType<CustomFailureInfo>(enriched.Failure);
        Assert.Equal("CUSTOM", failure.Code);
        Assert.True(failure.HasValue);
        Assert.Null(failure.Value);
        Assert.Equal("", failure.ResponseText);
        Assert.Null(metadata.InputText);
    }

    [Fact]
    public void SuccessfulResultsKeepTheirTypedValueAndNoFailureContext()
    {
        Example value = new Example { Value = 42 };
        var result = new Result<Example>(value);
        Assert.True(result.Success);
        Assert.Same(value, result.Value);
        Assert.Null(result.Failure);
        Assert.Throws<InvalidOperationException>(() => result.WithFailure(new FailureInfo()));
    }

    [Fact]
    public void JsonValidatorsRetainRejectedJsonWithAnIndependentLifetime()
    {
        var validator = new TypeValidator<Example>();
        string json = "{\"Value\":\"invalid\"}";
        Result<Example> result = validator.Validate(json);
        Assert.False(result.Success);
        Assert.True(result.Failure.HasValue);
        JsonElement rejected = Assert.IsType<JsonElement>(result.Failure.Value);
        Assert.Equal("invalid", rejected.GetProperty("Value").GetString());
        Assert.Equal(json, result.Failure.InputText);
        Assert.Null(result.Failure.ResponseText);
    }

    [Theory]
    [InlineData("{\"Value\":}")]
    [InlineData("")]
    public void JsonValidatorsRetainMalformedInputWithoutInventingAValue(string json)
    {
        Result<Example> result = new TypeValidator<Example>().Validate(json);
        Assert.False(result.Success);
        Assert.False(result.Failure.HasValue);
        Assert.Equal(json, result.Failure.InputText);
    }

    [Fact]
    public void ValueTypeValidationFailuresRemainFailuresInsteadOfThrowingDuringConversion()
    {
        var schema = new TypeSchema(typeof(int), new SchemaText("type Number = number;", SchemaText.Languages.Typescript));
        var validator = new JsonSerializerTypeValidator<int>(schema);
        Result<int> result = validator.Validate("\"invalid\"");
        Assert.False(result.Success);
        Assert.Equal(0, result.Value);
        Assert.Equal("invalid", Assert.IsType<JsonElement>(result.Failure.Value).GetString());
    }

    [Fact]
    public void FailureParsingHonorsValidatorJsonOptions()
    {
        var options = JsonSerializerTypeValidator.DefaultOptions();
        options.MaxDepth = 8;
        var validator = new JsonSerializerTypeValidator<Example>(ExampleSchema(), options);
        Result<Example> result = validator.Validate("{/* comment */\"Value\":\"invalid\",}");
        Assert.False(result.Success);
        Assert.True(result.Failure.HasValue);
        Assert.Equal("invalid", Assert.IsType<JsonElement>(result.Failure.Value).GetProperty("Value").GetString());
    }

    [Fact]
    public async Task TranslationFailureContextDoesNotBypassTheValidatorDepthLimit()
    {
        var options = JsonSerializerTypeValidator.DefaultOptions();
        options.MaxDepth = 1;
        var validator = new JsonSerializerTypeValidator<Example>(ExampleSchema(), options);
        string response = "{\"Value\":{\"nested\":1}}";
        var translator = new JsonTranslator<Example>(new FixedModel(response), validator) { MaxRepairAttempts = 0 };
        TypeChatException error = await Assert.ThrowsAsync<TypeChatException>(() => translator.TranslateAsync("request"));
        Assert.False(error.Failure.HasValue);
        Assert.Equal(response, error.Failure.InputText);
        Assert.Equal(response, error.Failure.ResponseText);
    }

    [Theory]
    [InlineData("{/* comment */\"Value\":42}")]
    [InlineData("{\"Value\":42,}")]
    public async Task TranslationFailureContextDoesNotReparseRejectedJsonWithLooserOptions(string response)
    {
        var validator = new JsonSerializerTypeValidator<Example>(ExampleSchema(), new JsonSerializerOptions());
        var translator = new JsonTranslator<Example>(new FixedModel(response), validator) { MaxRepairAttempts = 0 };
        TypeChatException error = await Assert.ThrowsAsync<TypeChatException>(() => translator.TranslateAsync("request"));
        Assert.False(error.Failure.HasValue);
        Assert.Equal(response, error.Failure.InputText);
        Assert.Equal(response, error.Failure.ResponseText);
    }

    [Fact]
    public void ConstraintValidatorsRetainTheRejectedInstance()
    {
        Person person = new Person
        {
            Name = new Name { FirstName = "First", LastName = "Last" },
            Age = 125
        };
        Result<Person> result = new ConstraintsValidator<Person>().Validate(person);
        Assert.False(result.Success);
        Assert.Same(person, result.Failure.Value);
        Assert.Null(result.Value);
    }

    [Fact]
    public void ProgramValidationRetainsTheRejectedProgramAndInput()
    {
        string json = "{\"@steps\":[{\"@func\":\"missing\"}]}";
        Result<Program> result = new ProgramValidator<IMathAPI>(MathAPI.Default).Validate(json);
        Assert.False(result.Success);
        Assert.IsType<Program>(result.Failure.Value);
        Assert.Equal(json, result.Failure.InputText);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task TranslationFailuresRetainRejectedJsonAndFullModelText()
    {
        string response = "before {\"Value\":\"invalid\"} after";
        var model = new FixedModel(response);
        var translator = new JsonTranslator<Example>(model, new TypeValidator<Example>()) { MaxRepairAttempts = 0 };
        TypeChatException error = await Assert.ThrowsAsync<TypeChatException>(() => translator.TranslateAsync("request"));
        Assert.Equal(TypeChatException.ErrorCode.JsonValidation, error.Code);
        Assert.Equal(response, error.Response.ResponseText);
        Assert.Equal(response, error.Failure.ResponseText);
        Assert.Equal("{\"Value\":\"invalid\"}", error.Failure.InputText);
        Assert.Equal("invalid", Assert.IsType<JsonElement>(error.Failure.Value).GetProperty("Value").GetString());
        Assert.Single(model.Prompts);
    }

    [Theory]
    [InlineData("not JSON")]
    [InlineData("{\"Value\":")]
    [InlineData("{\"Value\":}")]
    [InlineData("")]
    public async Task MalformedOutputHasTextButNoInventedCandidate(string response)
    {
        var model = new FixedModel(response);
        TypeChatException error = await Assert.ThrowsAsync<TypeChatException>(() => Translator(model, 0).TranslateAsync("request"));
        Assert.False(error.Failure.HasValue);
        Assert.Equal(response, error.Failure.ResponseText);
    }

    [Fact]
    public async Task SuccessfulRepairAndDiagnosticEventsRemainUnchanged()
    {
        string first = "{\"Value\":\"invalid\"}";
        var model = new FixedModel(first, "{\"Value\":42}");
        var translator = new JsonTranslator<Example>(model, new TypeValidator<Example>());
        var responses = new List<string>();
        var repairs = new List<string>();
        translator.CompletionReceived += responses.Add;
        translator.AttemptingRepair += repairs.Add;
        Example result = await translator.TranslateAsync("request");
        Assert.Equal(42, result.Value);
        Assert.Equal(2, model.Prompts.Count);
        Assert.Contains(first, model.Prompts[1]);
        Assert.Equal(2, responses.Count);
        Assert.Single(repairs);
    }

    [Fact]
    public async Task FailedRepairReturnsTheLatestRejectedCandidate()
    {
        string second = "{\"Value\":\"second\"}";
        var model = new FixedModel("{\"Value\":\"first\"}", second);
        TypeChatException error = await Assert.ThrowsAsync<TypeChatException>(() => Translator(model).TranslateAsync("request"));
        Assert.Equal("second", Assert.IsType<JsonElement>(error.Failure.Value).GetProperty("Value").GetString());
        Assert.Equal(second, error.Failure.ResponseText);
        Assert.Equal(2, model.Prompts.Count);
    }

    [Theory]
    [InlineData("not JSON")]
    [InlineData("{\"Value\":")]
    [InlineData("{\"Value\":}")]
    public async Task MalformedRepairRetainsTheEarlierCandidateAndLatestText(string response)
    {
        var model = new FixedModel("{\"Value\":\"first\"}", response);
        TypeChatException error = await Assert.ThrowsAsync<TypeChatException>(() => Translator(model).TranslateAsync("request"));
        Assert.Equal("first", Assert.IsType<JsonElement>(error.Failure.Value).GetProperty("Value").GetString());
        Assert.Equal(response, error.Failure.ResponseText);
        Assert.Equal(response, error.Response.ResponseText);
        Assert.Equal(2, model.Prompts.Count);
    }

    [Fact]
    public async Task AdditionalValidationRetainsTheTransformedInstance()
    {
        var transformed = new Example { Value = 99 };
        var translator = new JsonTranslator<Example>(
            new FixedModel("{\"Value\":\"input\"}"), new FixedValidator(new Result<Example>(transformed)))
        {
            ConstraintsValidator = new FixedConstraints(Result<Example>.Error("constraint failure")),
            MaxRepairAttempts = 0
        };
        TypeChatException error = await Assert.ThrowsAsync<TypeChatException>(() => translator.TranslateAsync("request"));
        Assert.Same(transformed, error.Failure.Value);
        Assert.Equal("constraint failure", error.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(false)]
    [InlineData(0)]
    [InlineData("")]
    public async Task CustomFailureValuesAndMetadataArePreservedWithoutMutation(object? value)
    {
        var metadata = new CustomFailureInfo { Value = value, ResponseText = "" };
        Result<Example> original = Result<Example>.Error("custom").WithFailure(metadata);
        var translator = new JsonTranslator<Example>(
            new FixedModel("{\"Value\":42}"), new FixedValidator(original))
        { MaxRepairAttempts = 0 };
        TypeChatException error = await Assert.ThrowsAsync<TypeChatException>(() => translator.TranslateAsync("request"));
        var failure = Assert.IsType<CustomFailureInfo>(error.Failure);
        Assert.Equal("CUSTOM", failure.Code);
        Assert.True(failure.HasValue);
        Assert.Same(value, failure.Value);
        Assert.Equal("", failure.ResponseText);
        Assert.Null(original.Failure.InputText);
        Assert.Null(metadata.InputText);
    }

    [Theory]
    [InlineData("not JSON")]
    [InlineData("{\"Value\":")]
    [InlineData("{\"Value\":}")]
    public async Task MalformedInitialOutputStillAllowsRepair(string response)
    {
        var model = new FixedModel(response, "{\"Value\":42}");
        var translator = new JsonTranslator<Example>(model, new TypeValidator<Example>());
        Assert.Equal(42, (await translator.TranslateAsync("request")).Value);
        Assert.Equal(2, model.Prompts.Count);
    }

    [Fact]
    public async Task MultipleRepairsRetainOnlyTheLatestCandidateAndExistingPromptWindow()
    {
        var model = new FixedModel("{\"Value\":\"first\"}", "{\"Value\":\"second\"}", "{\"Value\":\"third\"}");
        TypeChatException error = await Assert.ThrowsAsync<TypeChatException>(() => Translator(model, 2).TranslateAsync("request"));
        Assert.Equal("third", Assert.IsType<JsonElement>(error.Failure.Value).GetProperty("Value").GetString());
        Assert.Equal(3, model.Prompts.Count);
        Assert.Equal(model.Prompts[0].Length + 2, model.Prompts[1].Length);
        Assert.Equal(model.Prompts[1].Length, model.Prompts[2].Length);
        Assert.DoesNotContain("{\"Value\":\"first\"}", model.Prompts[2]);
    }

    [Fact]
    public async Task RepairHttpFailuresKeepTheirOriginalExceptionAndCandidateContext()
    {
        var original = new HttpRequestException("network failure");
        var model = new FixedModel("{\"Value\":\"invalid\"}", original);
        HttpRequestException error = await Assert.ThrowsAsync<HttpRequestException>(() => Translator(model).TranslateAsync("request"));
        Assert.Same(original, error);
        var failure = Assert.IsType<FailureInfo>(error.Data[FailureInfo.ExceptionDataKey]);
        Assert.Equal("invalid", Assert.IsType<JsonElement>(failure.Value).GetProperty("Value").GetString());
        Assert.Equal("{\"Value\":\"invalid\"}", failure.ResponseText);
    }

    [Fact]
    public async Task InitialHttpFailuresHaveNoInventedCandidateContext()
    {
        var original = new HttpRequestException("network failure");
        HttpRequestException error = await Assert.ThrowsAsync<HttpRequestException>(() =>
            Translator(new FixedModel(original)).TranslateAsync("request"));
        Assert.Same(original, error);
        Assert.False(error.Data.Contains(FailureInfo.ExceptionDataKey));
    }

    [Theory]
    [InlineData("io")]
    [InlineData("json")]
    [InlineData("typechat")]
    public async Task OtherExpectedRepairExceptionsRetainContextWithoutWrapping(string kind)
    {
        Exception original = kind switch
        {
            "io" => new IOException("response failure"),
            "json" => new JsonException("response failure"),
            "typechat" => new TypeChatException(TypeChatException.ErrorCode.Unknown, "request"),
            _ => throw new ArgumentException("Unexpected exception kind", nameof(kind))
        };
        var model = new FixedModel("{\"Value\":\"invalid\"}", original);
        Exception error = await Assert.ThrowsAnyAsync<Exception>(() => Translator(model).TranslateAsync("request"));
        Assert.Same(original, error);
        var failure = Assert.IsType<FailureInfo>(error.Data[FailureInfo.ExceptionDataKey]);
        Assert.Equal("invalid", Assert.IsType<JsonElement>(failure.Value).GetProperty("Value").GetString());
    }

    [Fact]
    public async Task ProgrammingExceptionsAreNotIntercepted()
    {
        var original = new InvalidOperationException("model bug");
        var model = new FixedModel("{\"Value\":\"invalid\"}", original);
        InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Translator(model).TranslateAsync("request"));
        Assert.Same(original, error);
        Assert.False(error.Data.Contains(FailureInfo.ExceptionDataKey));
    }

    [Fact]
    public async Task ExistingTransportFailureMetadataIsPreserved()
    {
        var original = new HttpRequestException("network failure");
        original.Data[FailureInfo.ExceptionDataKey] = new CustomFailureInfo { Value = null, ResponseText = "" };
        var model = new FixedModel("{\"Value\":\"invalid\"}", original);
        HttpRequestException error = await Assert.ThrowsAsync<HttpRequestException>(() => Translator(model).TranslateAsync("request"));
        var failure = Assert.IsType<CustomFailureInfo>(error.Data[FailureInfo.ExceptionDataKey]);
        Assert.Equal("CUSTOM", failure.Code);
        Assert.True(failure.HasValue);
        Assert.Null(failure.Value);
        Assert.Equal("", failure.ResponseText);
    }

    [Fact]
    public async Task CancellationRemainsTheOriginalCancellationException()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var original = new OperationCanceledException(cancellation.Token);
        var model = new FixedModel("{\"Value\":\"invalid\"}", original);
        OperationCanceledException error = await Assert.ThrowsAsync<OperationCanceledException>(() =>
            Translator(model).TranslateAsync("request", cancellation.Token));
        Assert.Same(original, error);
        Assert.Equal(cancellation.Token, error.CancellationToken);
        Assert.False(error.Data.Contains(FailureInfo.ExceptionDataKey));
    }

    [Fact]
    public async Task FailureContextDoesNotLeakIntoLaterTranslations()
    {
        var second = new HttpRequestException("second request");
        var model = new FixedModel("{\"Value\":\"invalid\"}", new HttpRequestException("repair"), second);
        JsonTranslator<Example> translator = Translator(model);
        await Assert.ThrowsAsync<HttpRequestException>(() => translator.TranslateAsync("first"));
        HttpRequestException error = await Assert.ThrowsAsync<HttpRequestException>(() => translator.TranslateAsync("second"));
        Assert.Same(second, error);
        Assert.False(error.Data.Contains(FailureInfo.ExceptionDataKey));
    }
}
