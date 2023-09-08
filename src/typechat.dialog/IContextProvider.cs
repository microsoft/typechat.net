// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Dialog;

public interface IContextProvider
{
    IEnumerable<IPromptSection>? GetContext(string request);
}
