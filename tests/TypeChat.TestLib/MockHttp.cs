// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Tests;

public class MockHttpHandler : HttpMessageHandler
{
    public MockHttpHandler(string? jsonResponse = null)
    {
        JsonResponse = jsonResponse;
    }

    public MockHttpHandler(int statusCode = 0)
    {
        StatusCode = statusCode;
    }

    public int StatusCode { get; set; } = 0;
    public string? JsonResponse { get; set; } = null;
    public HttpResponseMessage? Response { get; set; }


    public int RequestCount { get; set; } = 0;
    public HttpRequestMessage? LastRequest { get; set; }
    public string? LastRequestBody { get; private set; }

    public void Clear()
    {
        RequestCount = 0;
        LastRequest = null;
        LastRequestBody = null;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;
        LastRequestBody = request.Content is null ? null : await request.Content.ReadAsStringAsync().ConfigureAwait(false);
        RequestCount++;
        HttpResponseMessage response = Response;
        if (response is null)
        {
            response = new HttpResponseMessage();
            if (!string.IsNullOrEmpty(JsonResponse))
            {
                response.SetJson(JsonResponse);
            }
            if (StatusCode > 0)
            {
                response.StatusCode = (HttpStatusCode)StatusCode;
            }
        }

        return response;
    }

    public static MockHttpHandler ErrorResponder(int statusCode)
    {
        return new MockHttpHandler(statusCode);
    }
}
