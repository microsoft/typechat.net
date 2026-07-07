// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;
using System.Net.Http;

/// <summary>
/// Extension methods for working with Http 
/// </summary>
internal static class HttpEx
{
    internal static async Task<Response> GetJsonResponseAsync<Request, Response>(this HttpClient client, string endpoint, Request request, int maxRetries, int retryPauseMs, string? apiToken = null)
    {
        var requestMessage = Json.ToJsonMessage(request);
        int retryCount = 0;
        while (true)
        {
            HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = requestMessage
            };
            try
            {
                if (!string.IsNullOrEmpty(apiToken))
                {
                    httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);
                }
                HttpResponseMessage response = await client.SendAsync(httpRequest).ConfigureAwait(false);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                    return Json.Parse<Response>(stream);
                }
                if (!response.StatusCode.IsTransientError() || retryCount >= maxRetries)
                {
                    // Let HttpClient throw an exception
                    response.EnsureSuccessStatusCode();
                    break;
                }
                if (retryPauseMs > 0)
                {
                    await Task.Delay(retryPauseMs).ConfigureAwait(false);
                }
                retryCount++;
            }
            finally
            {
                httpRequest.Dispose();
            }
        }
        return default;
    }

    /// <summary>
    /// Like <see cref="GetJsonResponseAsync"/>, but also returns the raw (unparsed) JSON response body
    /// so callers can surface it (for example, on <see cref="CompletionInfo.Raw"/>).
    /// </summary>
    internal static async Task<(TResponse Response, string Raw)> GetJsonResponseWithRawAsync<TRequest, TResponse>(this HttpClient client, string endpoint, TRequest request, int maxRetries, int retryPauseMs, string? apiToken = null)
    {
        var requestMessage = Json.ToJsonMessage(request);
        int retryCount = 0;
        while (true)
        {
            HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = requestMessage
            };
            try
            {
                if (!string.IsNullOrEmpty(apiToken))
                {
                    httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);
                }
                HttpResponseMessage response = await client.SendAsync(httpRequest).ConfigureAwait(false);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string raw = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return (Json.Parse<TResponse>(raw), raw);
                }
                if (!response.StatusCode.IsTransientError() || retryCount >= maxRetries)
                {
                    // Let HttpClient throw an exception
                    response.EnsureSuccessStatusCode();
                    break;
                }
                if (retryPauseMs > 0)
                {
                    await Task.Delay(retryPauseMs).ConfigureAwait(false);
                }
                retryCount++;
            }
            finally
            {
                httpRequest.Dispose();
            }
        }
        return default;
    }

    internal static bool IsTransientError(this HttpStatusCode status)
    {
        switch (status)
        {
            default:
                return false;

            case (HttpStatusCode)429: // Too many requests
            case HttpStatusCode.InternalServerError:
            case HttpStatusCode.BadGateway:
            case HttpStatusCode.ServiceUnavailable:
            case HttpStatusCode.GatewayTimeout:
                break;
        }

        return true;
    }
}
