﻿// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Tests;

public class MockHttpHandler : HttpMessageHandler
{
    public MockHttpHandler(string jsonResponse)
    {
        Response = new HttpResponseMessage();
        Response.SetJson(jsonResponse);
    }
    public MockHttpHandler(HttpResponseMessage response = null)
    {
        Response = response;
    }

    public int RequestCount { get; set; } = 0;
    public HttpResponseMessage Response { get; set; }

    public void Clear()
    {
        RequestCount = 0;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        RequestCount++;
        return Task.FromResult(Response);
    }

    public static MockHttpHandler ErrorResponder(int statusCode)
    {
        HttpResponseMessage response = new HttpResponseMessage { StatusCode =(HttpStatusCode) statusCode };
        return new MockHttpHandler(response);
    }
}
