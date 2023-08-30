// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public interface IJsonTranslatorPrompts
{
    string CreateRequestPrompt(TypeSchema schema, string request);
    string CreateRepairPrompt(TypeSchema schema, string json, string validationError);
}
