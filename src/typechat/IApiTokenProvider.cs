// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public interface IApiTokenProvider
{
    Task<string> GetAccessTokenAsync(CancellationToken cancelToken);
}
