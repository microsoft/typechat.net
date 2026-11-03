// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// JsonTranslator translates natural language requests into objects of type T.
/// 
/// Translation works as follows:
/// - The language model - which works in text - translates the request into JSON. JsonTranslator gives the model the schema describing the
/// structure of the JSON it should emit.
/// - This schema is typically expressed using the Typescript language, which was designed to define schema consisely.
/// - JsonTranslator can automatically generate Typescript schema for T. But you can also create schema in other ways.
/// - The model returns JSON.
/// - JsonTranslator uses Validators to validate and deserialize the returned JSON into a valid object of type T
/// - Optional ConstraintsValidators can also further valdidate the object
/// 
/// Since language models are stochastic, the returned JSON can have errors or fail type checks.
/// When this happens, JsonTranslator tries to REPAIR the JSON by sending translation errors back to the language model.
/// JsonTranslator will attempt repairs MaxRepairAttempts number of times.
/// 
/// </summary>
/// <typeparam name="T">Type to translate requests into</typeparam>
public class JsonTranslator<T> : IJsonTranslator
{
    public const int DefaultMaxRepairAttempts = 1;

    private readonly ILanguageModel _model;
    private IJsonTypeValidator<T> _validator;
    private IConstraintsValidator<T>? _constraintsValidator;
    private IJsonTranslatorPrompts _prompts;
    private readonly TranslationSettings _translationSettings;
    private int _maxRepairAttempts;

    /// <summary>
    /// Creates a new JsonTranslator that translates natural language requests into objects of type T
    /// </summary>
    /// <param name="model">The language model to use for translation</param>
    /// <param name="schema">Text schema for type T</param>
    public JsonTranslator(ILanguageModel model, SchemaText schema)
        : this(model, new JsonSerializerTypeValidator<T>(schema))
    {
    }

    /// <summary>
    /// Creates a new JsonTranslator that translates natural language requests into objects of type T
    /// </summary>
    /// <param name="model">The language model to use for translation</param>
    /// <param name="validator">Type validator to use to ensure that JSON returned by LLM can be transformed into T</param>
    /// <param name="prompts">(Optional) Customize Typechat prompts</param>
    public JsonTranslator(
        ILanguageModel model,
        IJsonTypeValidator<T> validator,
        IJsonTranslatorPrompts? prompts = null)
    {

        ArgumentVerify.ThrowIfNull(model, nameof(model));
        ArgumentVerify.ThrowIfNull(validator, nameof(validator));

        _model = model;

        _validator = validator;
        prompts ??= JsonTranslatorPrompts.Default;
        _prompts = prompts;
        _translationSettings = new TranslationSettings(); // Default settings
        _maxRepairAttempts = DefaultMaxRepairAttempts;
    }

    /// <summary>
    /// Create a new, customized JsonTranslator
    /// </summary>
    /// <param name="model">The language model to use for translation</param>
    /// <param name="prompts">Custom prompts to use during translation</param>
    /// <param name="knownVocabs">Any known vocabularies. JsonVocab attributes can bind to these during JSON deserialiation</param>
    public JsonTranslator(ILanguageModel model, IJsonTranslatorPrompts? prompts = null, IVocabCollection? knownVocabs = null)
    {
        ArgumentVerify.ThrowIfNull(model, nameof(model));

        _model = model;
        _validator = new TypeValidator<T>(knownVocabs);
        prompts ??= JsonTranslatorPrompts.Default;
        _prompts = prompts;
        _translationSettings = new TranslationSettings(); // Default settings
        _maxRepairAttempts = DefaultMaxRepairAttempts;
    }

    /// <summary>
    /// The language model doing the translation
    /// </summary>
    public ILanguageModel Model => _model;

    /// <summary>
    /// The associated Json validator
    /// </summary>
    public IJsonTypeValidator<T> Validator
    {
        get => _validator;
        set
        {
            ArgumentVerify.ThrowIfNull(value, nameof(Validator));
            _validator = value;
        }
    }

    /// <summary>
    /// Optional constraints validation, once a valid object of type T is available
    /// </summary>
    public IConstraintsValidator<T>? ConstraintsValidator
    {
        get => _constraintsValidator;
        set => _constraintsValidator = value;
    }

    /// <summary>
    /// Prompts used during translation
    /// </summary>
    public IJsonTranslatorPrompts Prompts
    {
        get => _prompts;
        set
        {
            value ??= JsonTranslatorPrompts.Default;
            _prompts = value;
        }
    }

    /// <summary>
    /// Translation settings. Use this to customize attributes like MaxTokens emitted
    /// </summary>
    public TranslationSettings TranslationSettings => _translationSettings;

    /// <summary>
    /// When > 0, JsonValidator will attempt to repair Json objects that fail to validate.
    /// By default, will make at least 1 attempt
    /// </summary>
    public int MaxRepairAttempts
    {
        get => _maxRepairAttempts;
        set
        {
            if (value < 0)
            {
                value = 0;
            }
            _maxRepairAttempts = value;
        }
    }

    // 
    // Subscribe to diagnostic and progress Events
    //

    /// <summary>
    /// Sending a prompt to the model
    /// </summary>
    public event Action<Prompt> SendingPrompt;

    /// <summary>
    /// Raw response from the model
    /// </summary>
    public event Action<string> CompletionReceived;

    /// <summary>
    /// Attempting repair with the given validation errors
    /// </summary>
    public event Action<string> AttemptingRepair;

    /// <summary>
    /// Translate a natural language request into an object'
    /// </summary>
    /// <param name="request">text request</param>
    /// <param name="cancelToken">optional cancel token</param>
    /// <returns></returns>
    public async Task<object> TranslateToObjectAsync(string request, CancellationToken cancelToken)
    {
        return await TranslateAsync(request, cancelToken);
    }

    /// <summary>
    /// Translate a natural language request into an object of type 'T'
    /// Final validation failures retain unvalidated output in TypeChatException.Failure.
    /// </summary>
    /// <param name="request">text request</param>
    /// <param name="cancelToken">optional cancel token</param>
    /// <returns>Result containing object of type T</returns>
    /// <exception cref="TypeChatException"></exception>
    public Task<T> TranslateAsync(string request, CancellationToken cancelToken = default)
    {
        return TranslateAsync(request, null, null, cancelToken);
    }

    /// <summary>
    /// Translate a natural language request into a <see cref="Result{T}"/> of type 'T'.
    /// Exhausted validation failures are returned with failure context rather than thrown.
    /// Transport errors and cancellation still propagate. Completion metadata describes the
    /// final completion, not the accumulated usage of repair attempts.
    /// </summary>
    /// <param name="request">text request</param>
    /// <param name="cancelToken">optional cancel token</param>
    /// <returns>A <see cref="Result{T}"/> containing the object of type T, or a failure.</returns>
    public Task<Result<T>> TranslateToResultAsync(string request, CancellationToken cancelToken = default)
    {
        return TranslateToResultAsync(request, null, null, cancelToken);
    }

    /// <summary>
    /// Translate a natural language request into an object of type 'T'
    /// </summary>
    /// <param name="request"></param>
    /// <param name="preamble"></param>
    /// <param name="requestSettings"></param>
    /// <param name="cancelToken"></param>
    /// <returns>Result containing object of type T</returns>
    /// <exception cref="TypeChatException"></exception>
    public async Task<T> TranslateAsync(
        Prompt request,
        IList<IPromptSection>? preamble,
        TranslationSettings? requestSettings = null,
        CancellationToken cancelToken = default
    )
    {
        Result<T> result = await TranslateWorkerAsync(request, preamble, requestSettings, GetLegacyResponseAsync, throwOnFailure: true, cancelToken).ConfigureAwait(false);
        return result.Value;
    }

    /// <summary>
    /// Translate a natural language request into a <see cref="Result{T}"/> of type 'T'. Unlike
    /// <see cref="TranslateAsync(Prompt, IList{IPromptSection}, TranslationSettings, CancellationToken)"/>,
    /// this method returns a failed <see cref="Result{T}"/> when validation and repair fail.
    /// Transport errors, cancellation, and unexpected exceptions still propagate.
    /// The result's <see cref="Result{T}.Info"/> describes the final completion, when available,
    /// and <see cref="Result{T}.Failure"/> retains rejected output on validation failure.
    /// This opt-in API uses <see cref="GetResponseWithMetadataAsync"/>; existing
    /// <see cref="TranslateAsync(Prompt, IList{IPromptSection}, TranslationSettings, CancellationToken)"/>
    /// calls continue to use <see cref="GetResponseAsync"/>.
    /// </summary>
    /// <param name="request">natural language request</param>
    /// <param name="preamble">optional preamble prepended to the prompt</param>
    /// <param name="requestSettings">optional translation settings</param>
    /// <param name="cancelToken">optional cancel token</param>
    /// <returns>A <see cref="Result{T}"/> containing the object of type T, or a failure.</returns>
    public Task<Result<T>> TranslateToResultAsync(
        Prompt request,
        IList<IPromptSection>? preamble = null,
        TranslationSettings? requestSettings = null,
        CancellationToken cancelToken = default
    )
    {
        return TranslateWorkerAsync(request, preamble, requestSettings, GetResponseWithMetadataAsync, throwOnFailure: false, cancelToken);
    }

    private async Task<Result<T>> TranslateWorkerAsync(
        Prompt request,
        IList<IPromptSection>? preamble,
        TranslationSettings? requestSettings,
        Func<Prompt, TranslationSettings, CancellationToken, Task<LanguageModelResponse>> getResponse,
        bool throwOnFailure,
        CancellationToken cancelToken
    )
    {
        ArgumentVerify.ThrowIfNull(request, nameof(request));

        requestSettings ??= _translationSettings;
        Prompt prompt = CreateRequestPrompt(request, preamble);
        int repairAttempts = 0;
        FailureInfo? lastFailure = null;
        while (true)
        {
            LanguageModelResponse modelResponse;
            try
            {
                modelResponse = await getResponse(prompt, requestSettings, cancelToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (lastFailure is not null &&
                ex is HttpRequestException or IOException or JsonException or TypeChatException)
            {
                lastFailure.AttachTo(ex);
                throw;
            }
            string responseText = modelResponse.Text;

            JsonResponse jsonResponse = JsonResponse.Parse(responseText);
            Result<T> validationResult;
            if (jsonResponse.HasCompleteJson)
            {
                validationResult = ValidateJson(jsonResponse.Json);
                if (validationResult.Success)
                {
                    validationResult.Info = MergeInfo(modelResponse.Info, repairAttempts);
                    return validationResult;
                }
            }
            else if (jsonResponse.HasJson)
            {
                // Partial json
                validationResult = Result<T>.Error(TypeChatException.IncompleteJson(jsonResponse));
            }
            else
            {
                validationResult = Result<T>.Error(TypeChatException.NoJson(jsonResponse));
            }

            validationResult = validationResult.WithFailure(new FailureInfo { ResponseText = responseText });
            if (lastFailure is not null)
            {
                validationResult = validationResult.WithFailure(lastFailure);
            }
            lastFailure = validationResult.Failure;
            validationResult.Info = MergeInfo(modelResponse.Info, repairAttempts);

            // Attempt to repair the Json that was returned
            ++repairAttempts;
            if (repairAttempts > _maxRepairAttempts)
            {
                if (throwOnFailure)
                {
                    TypeChatException.ThrowJsonValidation(request, jsonResponse, validationResult.Message, lastFailure);
                }
                return validationResult;
            }
            NotifyEvent(AttemptingRepair, validationResult.Message);

            PromptSection repairPrompt = CreateRepairPrompt(responseText, validationResult);
            if (repairAttempts > 1)
            {
                // Remove the previous attempts
                prompt.Trim(2);
            }
            prompt.AppendResponse(responseText);
            prompt.Append(repairPrompt);
        }
    }

    private static CompletionInfo MergeInfo(CompletionInfo? info, int repairAttempts)
    {
        return info?.WithRepairAttempts(repairAttempts) ?? new CompletionInfo { RepairAttempts = repairAttempts };
    }

    protected virtual Prompt CreateRequestPrompt(Prompt request, IList<IPromptSection> preamble)
    {
        return _prompts.CreateRequestPrompt(_validator.Schema, request, preamble);
    }

    private async Task<LanguageModelResponse> GetLegacyResponseAsync(Prompt prompt, TranslationSettings requestSettings, CancellationToken cancelToken)
    {
        string text = await GetResponseAsync(prompt, requestSettings, cancelToken).ConfigureAwait(false);
        return new LanguageModelResponse(text);
    }

    protected virtual async Task<string> GetResponseAsync(Prompt prompt, TranslationSettings requestSettings, CancellationToken cancelToken)
    {
        NotifyEvent(SendingPrompt, prompt);
        string responseText = await _model.CompleteAsync(prompt, requestSettings, cancelToken).ConfigureAwait(false);
        NotifyEvent(CompletionReceived, responseText);
        return responseText;
    }

    /// <summary>
    /// Completion hook for TranslateToResultAsync. Override this hook to customize the new
    /// metadata-aware path; existing GetResponseAsync overrides still serve TranslateAsync.
    /// </summary>
    protected virtual async Task<LanguageModelResponse> GetResponseWithMetadataAsync(Prompt prompt, TranslationSettings requestSettings, CancellationToken cancelToken)
    {
        NotifyEvent(SendingPrompt, prompt);
        LanguageModelResponse response = await _model.CompleteWithMetadataAsync(prompt, requestSettings, cancelToken).ConfigureAwait(false);
        NotifyEvent(CompletionReceived, response.Text);
        return response;
    }

    protected virtual PromptSection CreateRepairPrompt(string responseText, Result<T> validationResult)
    {
        return _prompts.CreateRepairPrompt(_validator.Schema, responseText, validationResult.Message);
    }

    // Return false if translation loop should stop
    protected virtual bool OnValidationComplete(Result<T> validationResult) { return true; }

    private Result<T> ValidateJson(string json)
    {
        var result = Validator.Validate(json);
        if (!result.Success)
        {
            result = result.WithFailure(result.Failure?.HasValue == true || result.Failure?.InputText is not null ?
                new FailureInfo { InputText = json } :
                FailureInfo.FromJson(json, JsonSerializerTypeValidator.DefaultOptions()));
        }
        if (!OnValidationComplete(result))
        {
            return result;
        }
        if (result.Success)
        {
            T instance = result.Value;
            result = (_constraintsValidator is not null) ?
                     _constraintsValidator.Validate(result.Value) :
                     result;
            if (!result.Success)
            {
                result = result.WithFailure(new FailureInfo { Value = instance, InputText = json });
            }
        }
        return result;
    }

    protected void NotifyEvent(Action<Prompt> evt, Prompt prompt)
    {
        if (evt is not null)
        {
            try
            {
                evt(prompt);
            }
            catch { }
        }
    }

    protected void NotifyEvent(Action<string> evt, string value)
    {
        if (evt is not null)
        {
            try
            {
                evt(value);
            }
            catch { }
        }
    }
}
