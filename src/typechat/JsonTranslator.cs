// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public class JsonTranslator<T>
{
    ILanguageModel _model;
    IJsonTranslatorPrompts _prompts;
    IJsonTypeValidator<T> _validator;
    RequestSettings _requestSettings;

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

    public bool AttemptRepair { get; set; } = true;

    public event Action<string> SendingPrompt;
    public event Action<string> CompletionReceived;
    public event Action<string> AttemptingRepair;

    /// <summary>
    /// Translate a natural language request into an object of type 'T'
    /// </summary>
    /// <param name="request"></param>
    /// <param name="requestSettings"></param>
    /// <param name="cancelToken"></param>
    /// <returns>object of type T</returns>
    /// <exception cref="TypeChatException"></exception>
    public async Task<T> TranslateAsync(
        string request,
        RequestSettings? requestSettings = null,
        CancellationToken cancelToken = default
        )
    {
        requestSettings ??= _requestSettings;
        string prompt = _prompts.CreateRequestPrompt(_validator.Schema, request);
        bool attemptRepair = AttemptRepair;
        while (true)
        {
            string responseText = await CompleteAsync(prompt, requestSettings, cancelToken).ConfigureAwait(false);
            string jsonText = GetJson(responseText);
            ValidationResult<T> validation = Validator.Validate(responseText);
            if (validation.Success)
            {
                return validation.Value;
            }
            if (!attemptRepair)
            {
                throw new TypeChatException(TypeChatException.ErrorCode.JsonValidation, validation.Message);
            }

            NotifyEvent(AttemptingRepair, validation.Message);
            prompt += $"{responseText}\n{_prompts.CreateRepairPrompt(_validator.Schema, responseText, validation.Message)}";
            attemptRepair = false;
        }
    }

    public async Task<string> RepairJsonAsync(
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

    public virtual string CreateRequestPrompt(string request)
    {
        return JsonTranslatorPrompts.RequestPrompt(Validator.Schema.TypeName, Validator.Schema.Schema, request);
    }

    public virtual string CreateRepairPrompt(string validationError)
    {
        return JsonTranslatorPrompts.RepairPrompt(validationError);
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

    string GetJson(string json)
    {
        int iStartAt = json.IndexOf('{');
        int iEndAt = json.LastIndexOf('}');
        if (iStartAt < 0 || iEndAt < 0 || iStartAt >= iEndAt)
        {
            throw new JsonException("JSON parse error");
        }
        return json.Substring(iStartAt, iEndAt - iStartAt + 1);
    }
}
