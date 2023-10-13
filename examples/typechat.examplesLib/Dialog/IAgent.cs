// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Dialog;

/// <summary>
/// An agent responds to messages
/// </summary>
public interface IAgent
{
    /// <summary>
    /// Respond to the request
    /// </summary>
    /// <param name="request">request message</param>
    /// <param name="cancellationToken">optional cancel token</param>
    /// <returns>response message</returns>
    Task<Message> GetResponseMessageAsync(Message request, CancellationToken cancellationToken = default);
}
