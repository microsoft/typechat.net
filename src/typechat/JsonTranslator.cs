// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public class JsonTranslator<T>
{
    public const int DefaultMaxRepairAttempts = 1;

    ILanguageModel _model;
    IJsonTypeValidator<T> _validator;
    IJsonTranslatorPrompts _prompts;
    RequestSettings _requestSettings;
    int _maxRepairAttempts;

    public JsonTranslator(ILanguageModel model, SchemaText schema)
        : this(model, new JsonSerializerTypeValidator<T>(schema))
    {
    }

    public JsonTranslator(ILanguageModel model, IJsonTypeValidator<T> validator, IJsonTranslatorPrompts? prompts = null)
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

    public IJsonTypeValidator<T> Validator
    {
        get => _validator;
        set
        {
            ArgumentNullException.ThrowIfNull(value, nameof(Validator));
            _validator = value;
        }
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
    public virtual Task<T> TranslateAsync(string request, CancellationToken cancelToken = default)
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
        string request,
        IList<IPromptSection>? preamble,
        RequestSettings? requestSettings = null,
        CancellationToken cancelToken = default
        )
    {
        ArgumentNullException.ThrowIfNull(request);

        requestSettings ??= _requestSettings;
        PromptSection requestPrompt = CreateRequestPrompt(request);
        Prompt prompt = new Prompt(preamble, requestPrompt);

        int repairAttempts = 0;
        while (true)
        {
            NotifyEvent(SendingPrompt, prompt);
            string responseText = await _model.CompleteAsync(prompt, requestSettings, cancelToken).ConfigureAwait(false);
            NotifyEvent(CompletionReceived, responseText);

            JsonResponse jsonResponse = JsonResponse.Parse(responseText);
            if (!jsonResponse.HasJson)
            {
                TypeChatException.ThrowNoJson(request, jsonResponse);
            }

            Result<T> validationResult;
            if (jsonResponse.HasCompleteJson)
            {
                validationResult = Validator.Validate(jsonResponse.Json);
                if (!OnValidationComplete(validationResult) ||
                    validationResult.Success)
                {
                    return validationResult.Value;
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

            PromptSection repairPrompt = _prompts.CreateRepairPrompt(_validator.Schema, responseText, validationResult.Message);
            if (repairAttempts > 1)
            {
                // Remove the previous attempts
                prompt.Trim(2);
            }
            prompt.PushResponse(responseText);
            prompt.Push(repairPrompt);
        }
    }

    protected virtual string CreateRequestPrompt(string request)
    {
        return _prompts.CreateRequestPrompt(_validator.Schema, request);
    }

    // Return false if translation loop should stop
    protected virtual bool OnValidationComplete(Result<T> validationResult) { return true; }

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
