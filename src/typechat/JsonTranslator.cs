// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public class JsonTranslator<T>
{
    public const int DefaultMaxRepairAttempts = 1;

    ILanguageModel _model;
    IJsonTypeValidator<T> _validator;
    IConstraintsValidator<T>? _constraintsValidator;
    IJsonTranslatorPrompts _prompts;
    RequestSettings _requestSettings;
    int _maxRepairAttempts;

    public JsonTranslator(ILanguageModel model, SchemaText schema)
        : this(model, new JsonSerializerTypeValidator<T>(schema))
    {
    }

    public JsonTranslator(
        ILanguageModel model,
        IJsonTypeValidator<T> validator,
        IJsonTranslatorPrompts? prompts = null)
    {
        ArgumentNullException.ThrowIfNull(model, nameof(model));
        ArgumentNullException.ThrowIfNull(validator, nameof(validator));

        _model = model;

        _validator = validator;
        prompts ??= JsonTranslatorPrompts.Default;
        _prompts = prompts;
        _requestSettings = new RequestSettings(); // Default settings
        _maxRepairAttempts = DefaultMaxRepairAttempts;
    }

    public IJsonTranslatorPrompts Prompts => _prompts;

    public IJsonTypeValidator<T> Validator
    {
        get => _validator;
        set
        {
            ArgumentNullException.ThrowIfNull(value, nameof(Validator));
            _validator = value;
        }
    }

    public IConstraintsValidator<T>? ConstraintsValidator
    {
        get => _constraintsValidator;
        set => _constraintsValidator = value;
    }

    public ILanguageModel Model => _model;

    public RequestSettings RequestSettings => _requestSettings;

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

    public bool AttemptRepair
    {
        get => (_maxRepairAttempts > 0);
        set
        {
            MaxRepairAttempts = value ? DefaultMaxRepairAttempts : 0;
        }
    }

    public event Action<Prompt> SendingPrompt;
    public event Action<string> CompletionReceived;
    public event Action<string> AttemptingRepair;

    /// <summary>
    /// Translate a natural language request into an object of type 'T'
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancelToken"></param>
    /// <returns>Result containing object of type T</returns>
    /// <exception cref="TypeChatException"></exception>
    public Task<T> TranslateAsync(string request, CancellationToken cancelToken = default)
    {
        return TranslateAsync(request, null, null, cancelToken);
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
        RequestSettings? requestSettings = null,
        CancellationToken cancelToken = default
        )
    {
        ArgumentNullException.ThrowIfNull(request);

        requestSettings ??= _requestSettings;
        Prompt prompt = CreateRequestPrompt(request, preamble);
        int repairAttempts = 0;
        while (true)
        {
            string responseText = await GetResponseAsync(prompt, requestSettings, cancelToken).ConfigureAwait(false);

            JsonResponse jsonResponse = JsonResponse.Parse(responseText);
            if (!jsonResponse.HasJson)
            {
                TypeChatException.ThrowNoJson(request, jsonResponse);
            }

            Result<T> validationResult;
            if (jsonResponse.HasCompleteJson)
            {
                validationResult = ValidateJson(jsonResponse.Json);
                if (validationResult.Success)
                {
                    return validationResult;
                }
            }
            else
            {
                // Partial json
                validationResult = Result<T>.Error(TypeChatException.IncompleteJson(jsonResponse));
            }

            // Attempt to repair the Json that was returned
            ++repairAttempts;
            if (repairAttempts > _maxRepairAttempts)
            {
                TypeChatException.ThrowJsonValidation(request, jsonResponse, validationResult.Message);
            }
            NotifyEvent(AttemptingRepair, validationResult.Message);

            PromptSection repairPrompt = CreateRepairPrompt(responseText, validationResult);
            if (repairAttempts > 1)
            {
                // Remove the previous attempts
                prompt.Trim(2);
            }
            prompt.PushResponse(responseText);
            prompt.Push(repairPrompt);
        }
    }

    protected virtual Prompt CreateRequestPrompt(Prompt request, IList<IPromptSection> preamble)
    {
        return _prompts.CreateRequestPrompt(_validator.Schema, request, preamble);
    }

    protected virtual async Task<string> GetResponseAsync(Prompt prompt, RequestSettings requestSettings, CancellationToken cancelToken)
    {
        NotifyEvent(SendingPrompt, prompt);
        string responseText = await _model.CompleteAsync(prompt, requestSettings, cancelToken).ConfigureAwait(false);
        NotifyEvent(CompletionReceived, responseText);
        return responseText;
    }

    protected virtual PromptSection CreateRepairPrompt(string responseText, Result<T> validationResult)
    {
        return _prompts.CreateRepairPrompt(_validator.Schema, responseText, validationResult.Message);
    }

    // Return false if translation loop should stop
    protected virtual bool OnValidationComplete(Result<T> validationResult) { return true; }

    Result<T> ValidateJson(string json)
    {
        var result = Validator.Validate(json);
        if (!OnValidationComplete(result))
        {
            return result;
        }
        if (result.Success)
        {
            result = (_constraintsValidator != null) ?
                     _constraintsValidator.Validate(result.Value) :
                     result;
        }
        return result;
    }

    protected void NotifyEvent(Action<Prompt> evt, Prompt prompt)
    {
        if (evt != null)
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
        if (evt != null)
        {
            try
            {
                evt(value);
            }
            catch { }
        }
    }
}
