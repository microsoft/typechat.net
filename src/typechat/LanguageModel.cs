// Copyright (c) Microsoft. All rights reserved.

using System.Text.Json;

namespace Microsoft.TypeChat;

/// <summary>
/// A lightweight ILanguageModel implementation over OpenAI or Azure OpenAI Chat Completion or
/// Responses REST API endpoints
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
    public async Task<LanguageModelResponse> CompleteAsync(Prompt prompt, TranslationSettings? settings = null, CancellationToken cancelToken = default)
    {
        ArgumentVerify.ThrowIfNullOrEmpty<IPromptSection>(prompt, nameof(prompt));

        string apiToken = _config.HasTokenProvider ? await _config.ApiTokenProvider.GetAccessTokenAsync(cancelToken) : null;
        if (_useResponsesApi)
        {
            var responsesRequest = CreateResponsesRequest(prompt, settings);
            var (responsesResponse, responsesRaw) = await _client.GetJsonResponseWithRawAsync<ResponsesRequest, ResponsesResponse>(_endPoint, responsesRequest, _config.MaxRetries, _config.MaxPauseMs, apiToken).ConfigureAwait(false);
            return new LanguageModelResponse(responsesResponse.GetText(), BuildResponsesInfo(responsesResponse, responsesRaw));
        }

        var request = CreateRequest(prompt, settings);
        var (response, raw) = await _client.GetJsonResponseWithRawAsync<Request, Response>(_endPoint, request, _config.MaxRetries, _config.MaxPauseMs, apiToken).ConfigureAwait(false);
        return new LanguageModelResponse(response.GetText(), BuildChatInfo(response, raw));
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
                // Route to the Responses API regardless of how the endpoint is written.
                _endPoint = BuildAzureResponsesEndpoint(_config.Endpoint, _config.ApiVersion);
            }
            else if (_config.Endpoint.IndexOf(@"chat/completions", StringComparison.OrdinalIgnoreCase) >= 0)
            {
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
    /// Returns true when the given endpoint targets the OpenAI Responses API (path ends with
    /// "/responses", ignoring any query string).
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
        public string? model { get; set; }
        public Choice[] choices { get; set; }
        public Usage? usage { get; set; }

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

    private struct Message
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

    private struct Choice
    {
        public Message message { get; set; }
        public string? finish_reason { get; set; }
    }

    private struct Usage
    {
        public int? prompt_tokens { get; set; }
        public int? completion_tokens { get; set; }
        public int? total_tokens { get; set; }
        public TokenDetails? prompt_tokens_details { get; set; }
        public TokenDetails? completion_tokens_details { get; set; }
    }

    private struct TokenDetails
    {
        public int? cached_tokens { get; set; }
        public int? reasoning_tokens { get; set; }
    }

    private struct ResponsesRequest
    {
        public string? model { get; set; }
        public Message[] input { get; set; }
        public double? temperature { get; set; }
        public int? max_output_tokens { get; set; }

        public static ResponsesRequest Create(string? model, Prompt prompt, TranslationSettings settings)
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

    private struct ResponsesResponse
    {
        public string? model { get; set; }
        public string? status { get; set; }
        public OutputItem[] output { get; set; }
        public ResponsesUsage? usage { get; set; }
        public IncompleteDetails? incomplete_details { get; set; }

        public string GetText()
        {
            if (output is not null)
            {
                foreach (OutputItem item in output)
                {
                    if (item.type == "message" && item.content is not null)
                    {
                        foreach (OutputContent content in item.content)
                        {
                            if (content.type == "output_text" && content.text is not null)
                            {
                                return content.text;
                            }
                        }
                    }
                }
            }
            return string.Empty;
        }
    }

    private struct OutputItem
    {
        public string? type { get; set; }
        public string? role { get; set; }
        public OutputContent[] content { get; set; }
    }

    private struct OutputContent
    {
        public string? type { get; set; }
        public string? text { get; set; }
    }

    private struct IncompleteDetails
    {
        public string? reason { get; set; }
    }

    private struct ResponsesUsage
    {
        public int? input_tokens { get; set; }
        public int? output_tokens { get; set; }
        public int? total_tokens { get; set; }
        public TokenDetails? input_tokens_details { get; set; }
        public TokenDetails? output_tokens_details { get; set; }
    }

    private static CompletionInfo BuildChatInfo(Response response, string raw)
    {
        var info = new CompletionInfo
        {
            Model = response.model,
            Raw = ParseRaw(raw),
            FinishReason = NormalizeChatFinishReason((response.choices is not null && response.choices.Length > 0) ? response.choices[0].finish_reason : null)
        };
        if (response.usage is Usage u)
        {
            info.Usage = new TokenUsage(
                u.prompt_tokens ?? 0,
                u.completion_tokens ?? 0,
                u.total_tokens ?? 0,
                u.prompt_tokens_details?.cached_tokens,
                u.completion_tokens_details?.reasoning_tokens);
        }
        return info;
    }

    private static CompletionInfo BuildResponsesInfo(ResponsesResponse response, string raw)
    {
        var info = new CompletionInfo
        {
            Model = response.model,
            Raw = ParseRaw(raw),
            FinishReason = NormalizeResponsesFinishReason(response.status, response.incomplete_details?.reason)
        };
        if (response.usage is ResponsesUsage u)
        {
            info.Usage = new TokenUsage(
                u.input_tokens ?? 0,
                u.output_tokens ?? 0,
                u.total_tokens ?? 0,
                u.input_tokens_details?.cached_tokens,
                u.output_tokens_details?.reasoning_tokens);
        }
        return info;
    }

    private static JsonElement? ParseRaw(string raw)
    {
        if (string.IsNullOrEmpty(raw))
        {
            return null;
        }
        try
        {
            // Match the leniency of the response deserializer (real APIs are strict, but be forgiving).
            var options = new JsonDocumentOptions { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip };
            using JsonDocument doc = JsonDocument.Parse(raw, options);
            return doc.RootElement.Clone();
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static CompletionFinishReason? NormalizeChatFinishReason(string? reason)
    {
        return reason switch
        {
            "stop" => CompletionFinishReason.Stop,
            "length" => CompletionFinishReason.Length,
            "content_filter" => CompletionFinishReason.ContentFilter,
            "tool_calls" or "function_call" => CompletionFinishReason.ToolCalls,
            null => (CompletionFinishReason?)null,
            _ => CompletionFinishReason.Other,
        };
    }

    private static CompletionFinishReason? NormalizeResponsesFinishReason(string? status, string? incompleteReason)
    {
        return status switch
        {
            "completed" => CompletionFinishReason.Stop,
            "incomplete" => incompleteReason switch
            {
                "max_output_tokens" => CompletionFinishReason.Length,
                "content_filter" => CompletionFinishReason.ContentFilter,
                _ => CompletionFinishReason.Other,
            },
            null => (CompletionFinishReason?)null,
            _ => CompletionFinishReason.Other,
        };
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
