// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public abstract class JsonTranslator<T, TMessage>
{
    public const int DefaultMaxRepairAttempts = 1;

    IJsonTypeValidator<T> _validator;
    IJsonTranslatorPrompts _prompts;
    RequestSettings _requestSettings;
    int _maxRepairAttempts;

    public JsonTranslator(IJsonTypeValidator<T> validator, IJsonTranslatorPrompts? prompts = null)
    {
        ArgumentNullException.ThrowIfNull(validator, nameof(validator));

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
    /// <param name="requestSettings"></param>
    /// <param name="context"></param>
    /// <param name="cancelToken"></param>
    /// <returns>Result containing object of type T</returns>
    /// <exception cref="TypeChatException"></exception>
    public async Task<T> TranslateAsync(
        string request,
        IEnumerable<TMessage>? context,
        RequestSettings? requestSettings = null,
        CancellationToken cancelToken = default
        )
    {
        requestSettings ??= _requestSettings;
        string requestPrompt = CreateRequestPrompt(request);
        string prompt = requestPrompt;
        int repairAttempts = 0;
        while (true)
        {
            NotifyEvent(SendingPrompt, prompt);
            string responseText = await GetResponseAsync(prompt, context, requestSettings, cancelToken).ConfigureAwait(false);
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

            // Attempt to repair the Json that was sent
            ++repairAttempts;
            if (repairAttempts > _maxRepairAttempts)
            {
                TypeChatException.ThrowJsonValidation(request, jsonResponse, validationResult.Message);
            }
            NotifyEvent(AttemptingRepair, validationResult.Message);
            prompt = requestPrompt + $"{responseText}\n{_prompts.CreateRepairPrompt(_validator.Schema, responseText, validationResult.Message)}";
        }
    }

    protected abstract Task<string> GetResponseAsync(string prompt, IEnumerable<TMessage>? context, RequestSettings? settings, CancellationToken cancelToken);

    protected virtual string CreateRequestPrompt(string request)
    {
        return _prompts.CreateRequestPrompt(_validator.Schema, request);
    }

    // Return false if translation loop should stop
    protected virtual bool OnValidationComplete(Result<T> validationResult) { return true; }

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

public class JsonTranslator<T> : JsonTranslator<T, string>
{
    ILanguageModel _model;

    public JsonTranslator(ILanguageModel model, SchemaText schema)
        : this(model, new JsonSerializerTypeValidator<T>(schema))
    {
    }

    public JsonTranslator(ILanguageModel model, IJsonTypeValidator<T> validator, IJsonTranslatorPrompts? prompts = null)
        : base(validator, prompts)
    {
        ArgumentNullException.ThrowIfNull(model, nameof(model));
        _model = model;
    }

    public ILanguageModel Model => _model;

    protected override Task<string> GetResponseAsync(string prompt, IEnumerable<string>? context, RequestSettings? settings, CancellationToken cancelToken)
    {
        return _model.CompleteAsync(BuildRequest(prompt, context), settings, cancelToken);
    }

    string BuildRequest(string prompt, IEnumerable<string>? context)
    {
        if (context == null)
        {
            return prompt;
        }
        StringBuilder sb = new StringBuilder();
        foreach (var item in context)
        {
            sb.AppendLine(item);
        }
        sb.AppendLine(prompt);
        return sb.ToString();
    }
}
