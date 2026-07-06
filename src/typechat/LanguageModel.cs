// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// A lightweight ILanguageModel implementation over OpenAI or Azure OpenAI Chat Completion REST API endpoint
/// </summary>
public class LanguageModel : ILanguageModel, IDisposable
{
    private static readonly TranslationSettings s_defaultSettings = new TranslationSettings();

    private readonly OpenAIConfig _config;
    private readonly ModelInfo _model;
    private HttpClient _client;
    private string _endPoint;
    private bool _useResponsesApi;

    /// <summary>
    /// Create an OpenAILanguageModel object using the given OpenAIConfig
    /// config.EndPoint must support the Chat Completion API
    /// </summary>
    /// <param name="config">configuration to use</param>
    /// <param name="model">information about the target model</param>
    /// <param name="client">http client to use</param>
    public LanguageModel(OpenAIConfig config, ModelInfo? model = null, HttpClient? client = null)
    {
        ArgumentVerify.ThrowIfNull(config, nameof(config));
        config.Validate();

        _config = config;
        _model = model ?? _config.Model;
        _client = client ?? new HttpClient();
        ConfigureClient();
    }

    /// <summary>
    /// Information about the language model
    /// </summary>
    public ModelInfo ModelInfo => _model;

    /// <summary>
    /// Get a completion for the given prompt
    /// </summary>
    /// <param name="prompt">prompt</param>
    /// <param name="settings">translation settings such as temperature</param>
    /// <param name="cancelToken">cancellation token</param>
    /// <returns></returns>
    public async Task<string> CompleteAsync(Prompt prompt, TranslationSettings? settings = null, CancellationToken cancelToken = default)
    {
        ArgumentVerify.ThrowIfNullOrEmpty<IPromptSection>(prompt, nameof(prompt));

        string apiToken = _config.HasTokenProvider ? await _config.ApiTokenProvider.GetAccessTokenAsync(cancelToken) : null;
        if (_useResponsesApi)
        {
            var responsesRequest = CreateResponsesRequest(prompt, settings);
            var responsesResponse = await _client.GetJsonResponseAsync<ResponsesRequest, ResponsesResponse>(_endPoint, responsesRequest, _config.MaxRetries, _config.MaxPauseMs, apiToken).ConfigureAwait(false);
            return responsesResponse.GetText();
        }

        var request = CreateRequest(prompt, settings);
        var response = await _client.GetJsonResponseAsync<Request, Response>(_endPoint, request, _config.MaxRetries, _config.MaxPauseMs, apiToken).ConfigureAwait(false);
        return response.GetText();
    }

    private Request CreateRequest(Prompt prompt, TranslationSettings? settings = null)
    {
        var request = Request.Create(prompt, settings ?? s_defaultSettings);
        if (!_config.Azure)
        {
            request.model = _model.Name;
        }
        return request;
    }

    private ResponsesRequest CreateResponsesRequest(Prompt prompt, TranslationSettings? settings = null)
    {
        // The Responses API always carries the model/deployment in the request body.
        return ResponsesRequest.Create(_model.Name, prompt, settings ?? s_defaultSettings);
    }

    private void ConfigureClient()
    {
        if (_config.Azure)
        {
            bool wantResponses = _config.UseResponsesApi ?? EndpointTargetsResponses(_config.Endpoint);
            if (wantResponses)
            {
                // Route to the Responses API regardless of how the endpoint is written (base url,
                // a chat/completions url, or an explicit /responses url).
                _endPoint = BuildAzureResponsesEndpoint(_config.Endpoint, _config.ApiVersion);
            }
            else if (_config.Endpoint.IndexOf(@"chat/completions", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                // A fully qualified chat/completions endpoint was supplied - use it as-is.
                _endPoint = _config.Endpoint;
            }
            else
            {
                string path = $"openai/deployments/{_model.Name}/chat/completions?api-version={_config.ApiVersion}";
                _endPoint = new Uri(new Uri(_config.Endpoint), path).AbsoluteUri;
            }
            if (!_config.HasTokenProvider)
            {
                _client.DefaultRequestHeaders.Add("api-key", _config.ApiKey);
            }
        }
        else
        {
            bool wantResponses = _config.UseResponsesApi ?? EndpointTargetsResponses(_config.Endpoint);
            _endPoint = wantResponses ? BuildResponsesEndpoint(_config.Endpoint) : _config.Endpoint;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _config.ApiKey);
            if (!string.IsNullOrEmpty(_config.Organization))
            {
                _client.DefaultRequestHeaders.Add("OpenAI-Organization", _config.Organization);
            }
        }
        _useResponsesApi = _config.UseResponsesApi ?? EndpointTargetsResponses(_endPoint);
        if (_config.TimeoutMs > 0)
        {
            _client.Timeout = TimeSpan.FromMilliseconds(_config.TimeoutMs);
        }
    }

    /// <summary>
    /// Build an Azure OpenAI Responses API endpoint from any Azure endpoint. The Responses route is
    /// resource scoped (not deployment scoped): "{scheme}://{host}/openai/responses?api-version=...".
    /// The model/deployment is carried in the request body.
    /// </summary>
    private static string BuildAzureResponsesEndpoint(string endpoint, string apiVersion)
    {
        if (EndpointTargetsResponses(endpoint))
        {
            return endpoint;
        }
        Uri authority = new Uri(new Uri(endpoint).GetLeftPart(UriPartial.Authority));
        string path = string.IsNullOrEmpty(apiVersion) ? "openai/responses" : $"openai/responses?api-version={apiVersion}";
        return new Uri(authority, path).AbsoluteUri;
    }

    /// <summary>
    /// Build an OpenAI (non-Azure) Responses API endpoint. If the endpoint targets chat/completions,
    /// it is rewritten to the sibling "responses" route; otherwise "responses" is appended.
    /// </summary>
    private static string BuildResponsesEndpoint(string endpoint)
    {
        if (EndpointTargetsResponses(endpoint))
        {
            return endpoint;
        }
        const string chatCompletions = "chat/completions";
        int index = endpoint.IndexOf(chatCompletions, StringComparison.OrdinalIgnoreCase);
        if (index >= 0)
        {
            return endpoint.Substring(0, index) + "responses" + endpoint.Substring(index + chatCompletions.Length);
        }
        return endpoint.TrimEnd('/') + "/responses";
    }

    /// <summary>
    /// Returns true when the given endpoint targets the OpenAI Responses API.
    /// Detection is based on whether the url path ends with "/responses" (ignoring any query string).
    /// This covers both the standard OpenAI endpoint (https://api.openai.com/v1/responses) and
    /// Azure OpenAI endpoints that end with "/responses?api-version=...".
    /// </summary>
    internal static bool EndpointTargetsResponses(string endpoint)
    {
        if (string.IsNullOrEmpty(endpoint))
        {
            return false;
        }
        string path = endpoint;
        if (Uri.TryCreate(endpoint, UriKind.Absolute, out Uri uri))
        {
            path = uri.AbsolutePath;
        }
        else
        {
            int query = path.IndexOf('?');
            if (query >= 0)
            {
                path = path.Substring(0, query);
            }
        }
        path = path.TrimEnd('/');
        return path.EndsWith("/responses", StringComparison.OrdinalIgnoreCase);
    }

    private struct Request
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

    private struct Response
    {
        public ResponseChoice[] choices { get; set; }

        public string GetText()
        {
            string response = null;
            if (choices is not null && choices.Length > 0)
            {
                response = choices[0].message.content;
            }
            return response ?? string.Empty;
        }
    }

    // A request message. 'content' is either a string (text only) or an array of
    // ContentPart (multimodal). System.Text.Json serializes the object by its runtime type.
    private struct Message
    {
        public string role { get; set; }
        public object content { get; set; }

        public static Message[] Create(Prompt prompt)
        {
            Message[] messages = new Message[prompt.Count];
            for (int i = 0; i < prompt.Count; ++i)
            {
                messages[i] = Create(prompt[i]);
            }
            return messages;
        }

        public static Message Create(IPromptSection section)
        {
            return new Message { role = section.Source, content = CreateContent(section) };
        }

        private static object CreateContent(IPromptSection section)
        {
            if (section is IMultimodalPromptSection multimodal && HasImages(multimodal))
            {
                IReadOnlyList<PromptContentPart> parts = multimodal.ContentParts;
                ContentPart[] content = new ContentPart[parts.Count];
                for (int i = 0; i < parts.Count; ++i)
                {
                    content[i] = ContentPart.Create(parts[i]);
                }
                return content;
            }
            return section.GetText();
        }

        private static bool HasImages(IMultimodalPromptSection section)
        {
            IReadOnlyList<PromptContentPart> parts = section.ContentParts;
            for (int i = 0; i < parts.Count; ++i)
            {
                if (parts[i].IsImage)
                {
                    return true;
                }
            }
            return false;
        }
    }

    // A single content part for a Chat Completions multimodal message.
    private struct ContentPart
    {
        public string type { get; set; }
        public string? text { get; set; }
        public ImageUrl? image_url { get; set; }

        public static ContentPart Create(PromptContentPart part)
        {
            if (part.IsImage)
            {
                PromptImage image = part.Image!;
                return new ContentPart
                {
                    type = "image_url",
                    image_url = new ImageUrl
                    {
                        url = image.Url,
                        detail = DetailToString(image.Detail)
                    }
                };
            }
            return new ContentPart { type = "text", text = part.Text ?? string.Empty };
        }

        // Auto is the service default, so it is omitted from the wire format.
        private static string? DetailToString(ImageDetail detail)
        {
            switch (detail)
            {
                case ImageDetail.Low:
                    return "low";
                case ImageDetail.High:
                    return "high";
                default:
                    return null;
            }
        }
    }

    private struct ImageUrl
    {
        public string url { get; set; }
        public string? detail { get; set; }
    }

    private struct ResponseChoice
    {
        public ResponseMessage message { get; set; }
    }

    private struct ResponseMessage
    {
        public string role { get; set; }
        public string content { get; set; }
    }

    // Request for the OpenAI Responses API. Uses 'input' (array of messages) instead of 'messages',
    // and 'max_output_tokens' instead of 'max_tokens'.
    private struct ResponsesRequest
    {
        public string? model { get; set; }
        public Message[] input { get; set; }
        public double? temperature { get; set; }
        public int? max_output_tokens { get; set; }

        public static ResponsesRequest Create(string model, Prompt prompt, TranslationSettings settings)
        {
            return new ResponsesRequest
            {
                model = model,
                input = Message.Create(prompt),
                temperature = (settings.Temperature > 0) ? settings.Temperature : 0,
                max_output_tokens = (settings.MaxTokens > 0) ? settings.MaxTokens : null
            };
        }
    }

    // Response from the OpenAI Responses API. The text is returned inside output[n].content[m].text
    // where the matching output item has type == "message" and the content item has type == "output_text".
    private struct ResponsesResponse
    {
        public OutputItem[] output { get; set; }

        public string GetText()
        {
            if (output is not null)
            {
                for (int i = 0; i < output.Length; ++i)
                {
                    OutputItem item = output[i];
                    if (!string.Equals(item.type, "message", StringComparison.Ordinal) || item.content is null)
                    {
                        continue;
                    }
                    for (int j = 0; j < item.content.Length; ++j)
                    {
                        OutputContent content = item.content[j];
                        if (string.Equals(content.type, "output_text", StringComparison.Ordinal) && content.text is not null)
                        {
                            return content.text;
                        }
                    }
                }
            }
            return string.Empty;
        }
    }

    private struct OutputItem
    {
        public string type { get; set; }
        public string? role { get; set; }
        public OutputContent[] content { get; set; }
    }

    private struct OutputContent
    {
        public string type { get; set; }
        public string text { get; set; }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _client?.Dispose();
            _client = null;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
