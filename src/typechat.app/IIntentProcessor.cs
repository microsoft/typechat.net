// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public interface IIntentProcessor
{
    Task ProcessRequestAsync(string input, CancellationToken cancelToken);
}
