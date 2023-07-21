// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public class TypeChat<T>
{
    ICompletionModel _model;
    IJsonTypeValidator<T> _validator;
    RequestSettings _requestSettings;

    public TypeChat(ICompletionModel model, IJsonTypeValidator<T> validator)
    {
        ArgumentNullException.ThrowIfNull(model, nameof(model));
        ArgumentNullException.ThrowIfNull(validator, nameof(validator));
        _model = model;
        _validator = validator;
        _requestSettings = new RequestSettings(); // Default settings
    }

    public ICompletionModel Model => _model;
    public IJsonTypeValidator<T> Validator => _validator;
    public RequestSettings RequestSettings => _requestSettings;

    public bool AttemptRepair { get; set; } = true;

    public event Action<string> CompletionRequest;
    public event Action<string> CompletionReceived;

    /// <summary>
    /// Translate a natural language request into an object of type 'T'
    /// </summary>
    /// <param name="request"></param>
    /// <param name="requestSettings"></param>
    /// <param name="cancelToken"></param>
    /// <returns></returns>
    /// <exception cref="TypeChatException"></exception>
    public async Task<T> TranslateAsync(
        string request,
        RequestSettings? requestSettings = null,
        CancellationToken cancelToken = default
        )
    {
        requestSettings ??= _requestSettings;
        string prompt = CreateRequestPrompt(request);
        bool attemptRepair = AttemptRepair;
        while(true)
        {
            string completion = await CompleteAsync(prompt, requestSettings, cancelToken).ConfigureAwait(false);

            ValidationResult<T> validation = Validator.Validate(completion);
            if (validation.Success)
            {
                return validation.Value;
            }
            if (!attemptRepair)
            {
                throw new TypeChatException(TypeChatException.ErrorCode.JsonValidation, validation.Message);
            }
            prompt += $"{completion}\n{CreateRepairPrompt(validation.Message)}";
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

        string prompt = Prompts.RepairPrompt(json, _validator.Schema, validationError);
        return await CompleteAsync(prompt, settings, cancelToken).ConfigureAwait(false);
    }

    protected async virtual Task<string> CompleteAsync(string prompt, RequestSettings? settings, CancellationToken cancelToken)
    {
        NotifyEvent(CompletionRequest, prompt);
        string completion = await Model.CompleteAsync(prompt, settings, cancelToken).ConfigureAwait(false);
        NotifyEvent(CompletionReceived, completion);
        return completion;
    }

    protected virtual string CreateRequestPrompt(string request)
    {
        return Prompts.RequestPrompt(Validator.Schema.TypeName, Validator.Schema.Schema, request);
    }

    protected virtual string CreateRepairPrompt(string validationError)
    {
        return Prompts.RepairPrompt(validationError);
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
