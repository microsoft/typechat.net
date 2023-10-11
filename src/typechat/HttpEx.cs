// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// Extension methods for working with Http 
/// </summary>
internal static class HttpEx
{
    internal static async Task<Response> GetJsonResponse<Request, Response>(this HttpClient client, string endpoint, Request request, int maxRetries, int retryPauseMs)
    {
        var requestMessage = Json.ToJsonMessage(request);
        int retryCount = 0;
        while (true)
        {
            HttpResponseMessage response = await client.PostAsync(endpoint, requestMessage).ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                using Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                return JsonSerializer.Deserialize<Response>(stream);
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
