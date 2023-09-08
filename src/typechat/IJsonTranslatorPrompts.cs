﻿// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public interface IJsonTranslatorPrompts
{
    Prompt CreateRequestPrompt(TypeSchema schema, Prompt request, IList<IPromptSection> preamble);
    string CreateRepairPrompt(TypeSchema schema, string json, string validationError);
}
