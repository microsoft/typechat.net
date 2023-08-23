// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public class JsonTranslator<T>
{
    public const int DefaultMaxRepairAttempts = 1;

    ILanguageModel _model;
    IJsonTranslatorPrompts _prompts;
    IJsonTypeValidator<T> _validator;
    RequestSettings _requestSettings;
    int _maxRepairAttempts;

    public JsonTranslator(ILanguageModel model, SchemaText schema)
        : this(model, new JsonSerializerTypeValidator<T>(schema))
    {
    }

    public JsonTranslator(
        ILanguageModel model,
        IJsonTypeValidator<T> validator,
        IJsonTranslatorPrompts? prompts = null
        )
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

    public ILanguageModel Model => _model;
    public IJsonTypeValidator<T> Validator
    {
        get => _validator;
        set
        {
            ArgumentNullException.ThrowIfNull(value, nameof(Validator));
            _validator = value;
        }
    }

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

    public event Action<string> SendingPrompt;
    public event Action<string> CompletionReceived;
    public event Action<string> AttemptingRepair;

    /// <summary>
    /// Translate a natural language request into an object of type 'T'
    /// </summary>
    /// <param name="request"></param>
    /// <param name="requestSettings"></param>
    /// <param name="cancelToken"></param>
    /// <returns>Result containing object of type T</returns>
    /// <exception cref="TypeChatException"></exception>
    public async Task<Result<T>> TranslateAsync(
        string request,
        RequestSettings? requestSettings = null,
        CancellationToken cancelToken = default
        )
    {
        requestSettings ??= _requestSettings;
        string prompt = _prompts.CreateRequestPrompt(_validator.Schema, request);
        int repairAttempts = 0;
        while (true)
        {
            string responseText = await CompleteAsync(prompt, requestSettings, cancelToken).ConfigureAwait(false);
            JsonResponse response = JsonResponse.Parse(responseText);
            if (!response.HasJson)
            {
                return OnEmptyResponse(response);
            }

            Result<T> validationResult;
            if (response.HasCompleteJson)
            {
                validationResult = Validator.Validate(response.Json);
            }
            else
            {
                // Partial json
                validationResult = Result<T>.Error("Json Parse Error:\n" + response.Json);
            }
            if (!OnValidationComplete(response, validationResult) ||
                validationResult.Success)
            {
                return validationResult.Value;
            }

            // Attempt to repair the Json that was sent
            ++repairAttempts;
            if (repairAttempts > _maxRepairAttempts)
            {
                throw new TypeChatException(TypeChatException.ErrorCode.JsonValidation, validationResult.Message);
            }
            NotifyEvent(AttemptingRepair, validationResult.Message);
            prompt += $"{responseText}\n{_prompts.CreateRepairPrompt(_validator.Schema, responseText, validationResult.Message)}";
        }
    }

    protected async Task<string> RepairJsonAsync(
        string json,
        string validationError,
        RequestSettings? settings = null,
        CancellationToken cancelToken = default
        )
    {
        ArgumentException.ThrowIfNullOrEmpty(json, nameof(json));

        string prompt = JsonTranslatorPrompts.RepairPrompt(json, _validator.Schema, validationError);
        return await CompleteAsync(prompt, settings, cancelToken).ConfigureAwait(false);
    }

    protected async virtual Task<string> CompleteAsync(string prompt, RequestSettings? settings, CancellationToken cancelToken)
    {
        NotifyEvent(SendingPrompt, prompt);
        string completion = await Model.CompleteAsync(prompt, settings, cancelToken).ConfigureAwait(false);
        NotifyEvent(CompletionReceived, completion);
        return completion;
    }

    protected virtual string CreateRequestPrompt(string request)
    {
        return JsonTranslatorPrompts.RequestPrompt(Validator.Schema.TypeName, Validator.Schema.Schema, request);
    }

    protected virtual string CreateRepairPrompt(string validationError)
    {
        return JsonTranslatorPrompts.RepairPrompt(validationError);
    }

    protected virtual Result<T> OnEmptyResponse(JsonResponse response)
    {
        throw new TypeChatException(TypeChatException.ErrorCode.NoJson, response.Message());
    }
    // Return false if translation loop should stop
    protected virtual bool OnValidationComplete(JsonResponse response, Result<T> validationResult)
    {
        return true;
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
