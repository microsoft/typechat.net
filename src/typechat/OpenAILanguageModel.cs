// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public class OpenAILanguageModel : ILanguageModel
{
    static TranslationSettings s_defaultSettings = new TranslationSettings();

    OpenAIConfig _config;
    ModelInfo _model;
    HttpClient _client;
    string _endPoint;

    public OpenAILanguageModel(OpenAIConfig config, ModelInfo? model = null)
    {
        ArgumentVerify.ThrowIfNull(config, nameof(config));
        config.Validate();

        _config = config;
        _model = model ?? _config.Model;
        _client = new HttpClient();
        ConfigureClient();
    }

    public HttpClient Client => _client;

    public ModelInfo ModelInfo => throw new NotImplementedException();

    public async Task<string> CompleteAsync(Prompt prompt, TranslationSettings? settings, CancellationToken cancelToken)
    {
        ArgumentVerify.ThrowIfNullOrEmpty<IPromptSection>(prompt, nameof(prompt));

        var request = CreateRequest(prompt, settings);
        var response = await _client.GetJsonResponse<Request, Response>(_endPoint, request, _config.MaxRetries, _config.MaxPauseMs);
        return response.GetText();
    }

    Request CreateRequest(Prompt prompt, TranslationSettings? settings)
    {
        var request = Request.Create(prompt, settings ?? s_defaultSettings);
        if (!_config.Azure)
        {
            request.model = _model.Name;
        }
        return request;
    }
    void ConfigureClient()
    {
        if (_config.Azure)
        {
            string path = $"openai/deployments/{_model.Name}/chat/completions?api-version={_config.ApiVersion}";
            _endPoint = new Uri(new Uri(_config.Endpoint), path).AbsoluteUri;
            _client.DefaultRequestHeaders.Add("api-key", _config.ApiKey);
        }
        else
        {
            _endPoint = _config.Endpoint;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _config.ApiKey);
            if (!string.IsNullOrEmpty(_config.Organization))
            {
                _client.DefaultRequestHeaders.Add("OpenAI-Organization", _config.Organization);
            }
        }
    }

    struct Request
    {
        public string? model { get; set; }
        public Message[] messages { get; set; }
        public double? temperature { get; set; }
        public int? max_tokens { get; set; }

        public static Request Create(Prompt prompt, TranslationSettings settings)
        {
            return new Request
            {
                messages = Message.Create(prompt),
                temperature = (settings.Temperature > 0) ? settings.Temperature : 0,
                max_tokens = (settings.MaxTokens > 0) ? settings.MaxTokens : null
            };
        }
    }

    struct Response
    {
        public Choice[] choices { get; set; }

        public string GetText() => (choices != null && choices.Length > 0) ? choices[0].message.content : string.Empty;
    }

    struct Message
    {
        public string role { get; set; }
        public string content { get; set; }

        public static Message[] Create(Prompt prompt)
        {
            Message[] messages = new Message[prompt.Count];
            for (int i = 0; i < prompt.Count; ++i)
            {
                messages[i] = new Message { role = prompt[i].Source, content = prompt[i].GetText() };
            }
            return messages;
        }
    }

    struct Choice
    {
        public Message message { get; set; }
    }
}
