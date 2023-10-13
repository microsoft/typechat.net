// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// A TextRequestRouter routes a request to a target
/// </summary>
/// <typeparam name="T">Type of target</typeparam>
public interface ITextRequestRouter<T>
{
    Task<T> RouteRequestAsync(string request, CancellationToken cancellationToken = default);
}
