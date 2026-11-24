// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>Default program prompts: both the program grammar and the target API are system instructions.</summary>
public class SchemaFirstProgramTranslatorPrompts : SchemaFirstJsonTranslatorPrompts
{
    private readonly string _apiDef;

    public SchemaFirstProgramTranslatorPrompts(string apiDef)
    {
        ArgumentVerify.ThrowIfNullOrEmpty(apiDef, nameof(apiDef));
        _apiDef = apiDef;
    }

    /// <inheritdoc/>
    protected override string CreateSystemInstructions(TypeSchema schema) =>
        ProgramTranslatorPrompts.ProgramInstructions(schema.Schema.Text, _apiDef) + JsonFormatInstructions;
}
