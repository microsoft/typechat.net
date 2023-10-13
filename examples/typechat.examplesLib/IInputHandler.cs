// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// An input processor is a target for natural language input from the user
/// </summary>
public interface IInputHandler
{
    Task ProcessInputAsync(string input, CancellationToken cancellationToken = default);
}
