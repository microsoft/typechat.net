// Copyright (c) Microsoft. All rights reserved.

using Azure.Core;
using Azure.Identity;

namespace Microsoft.TypeChat;


public enum AzureTokenScopes
{
    CogServices = 0 // You can assign specific integer values if needed  
}

public class AzureTokenProvider : IApiTokenProvider, IDisposable
{
    public const int DefaultExpirationBufferMs = 5 * 60 * 1000;

    static AzureTokenProvider s_default;

    static AzureTokenProvider()
    {
        s_default = new AzureTokenProvider(AzureTokenScopes.CogServices);
    }
    public static AzureTokenProvider Default
    {
        get { return s_default; }
    }

    TokenCredential _credential;
    string[] _scopes;
    int _expirationBufferMs;
    AccessToken _accessToken;
    SemaphoreSlim _lock;

    public AzureTokenProvider(AzureTokenScopes scope, int expirationBufferMs = DefaultExpirationBufferMs)
        : this(GetScopes(scope), expirationBufferMs)
    {
    }

    public AzureTokenProvider(string[] scopes, int expirationBufferMs = DefaultExpirationBufferMs)
    {
        ArgumentVerify.ThrowIfNullOrEmpty(scopes, nameof(scopes));
        _credential = new DefaultAzureCredential();
        _scopes = scopes;
        _expirationBufferMs = expirationBufferMs;
        _lock = new SemaphoreSlim(1, 1);
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken cancelToken)
    {
        if (_accessToken.ExpiresOn <= DateTimeOffset.UtcNow.AddMilliseconds(_expirationBufferMs))
        {
            return await RefreshTokenAsync(cancelToken).ConfigureAwait(false);
        }
        return _accessToken.Token;
    }

    public async Task<string> RefreshTokenAsync(CancellationToken cancelToken)
    {
        await _lock.WaitAsync(cancelToken).ConfigureAwait(false);
        try
        {
            _accessToken = await _credential.GetTokenAsync(new TokenRequestContext(_scopes), cancelToken).ConfigureAwait(false);
            _accessToken = new AccessToken(_accessToken.Token, _accessToken.ExpiresOn.AddMilliseconds(-_expirationBufferMs));
            return _accessToken.Token;
        }
        finally
        {
            _lock.Release();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool fromDispose)
    {
        if (fromDispose)
        {
            _lock?.Dispose();
        }
        _lock = null;
    }

    static string[] GetScopes(AzureTokenScopes scope)
    {
        switch (scope)
        {
            case AzureTokenScopes.CogServices:
                return new string[] {
                    "https://cognitiveservices.azure.com/.default"
                };
            default:
                throw new ArgumentOutOfRangeException(nameof(scope), "Unsupported Azure token scope.");
        }
    }
}
