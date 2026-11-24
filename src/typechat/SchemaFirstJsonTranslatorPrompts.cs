// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// The default chat layout: application instructions, then the caller's preamble and request sections.
/// </summary>
public class SchemaFirstJsonTranslatorPrompts : JsonTranslatorPrompts
{
    /// <summary>Output format shared by the JSON and program translators.</summary>
    protected const string JsonFormatInstructions =
        "Return only the translated JSON object with 2 spaces of indentation and no properties with the value undefined.\n";

    /// <summary>Create application-owned instructions. This hook receives no request or history.</summary>
    protected virtual string CreateSystemInstructions(TypeSchema schema) =>
        SchemaInstructions(schema.TypeFullName, schema.Schema) + JsonFormatInstructions;

    /// <inheritdoc/>
    public sealed override Prompt CreateRequestPrompt(TypeSchema typeSchema, Prompt request, IList<IPromptSection>? context = null)
    {
        ArgumentVerify.ThrowIfNull(request, nameof(request));
        Prompt prompt = new Prompt(PromptSection.FromSystem(CreateSystemInstructions(typeSchema)));
        if (context is not null)
        {
            prompt.Append(context);
        }
        prompt.Append(request);
        return prompt;
    }
}
