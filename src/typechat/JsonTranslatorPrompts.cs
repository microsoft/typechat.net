// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// The standard prompts used by JsonTranslator
/// </summary>
public class JsonTranslatorPrompts : IJsonTranslatorPrompts
{
    internal static readonly JsonTranslatorPrompts Default = new JsonTranslatorPrompts();

    public virtual Prompt CreateRequestPrompt(TypeSchema schema, Prompt request, IList<IPromptSection> preamble = null)
    {
        return RequestPrompt(schema.TypeFullName, schema.Schema, request, preamble);
    }

    public virtual string CreateRepairPrompt(TypeSchema schema, string json, string validationError)
    {
        return RepairPrompt(validationError);
    }

    public static Prompt RequestPrompt(string typeName, string schema, Prompt request, IList<IPromptSection>? context = null)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));
        Prompt prompt = new Prompt();

        prompt += IntroSection(typeName, schema);
        AddContextAndRequest(prompt, request, context);

        return prompt;
    }

    public static Prompt AddContextAndRequest(Prompt prompt, Prompt request, IList<IPromptSection> preamble)
    {
        if (!preamble.IsNullOrEmpty())
        {
            prompt.Append(preamble);
        }

        if (request.Count == 1)
        {
            prompt += RequestSection(request[0].GetText());
            return prompt;
        }

        prompt += "USER REQUEST:";
        prompt.Append(request);
        prompt += "The following is USER REQUEST translated into a JSON object with 2 spaces of indentation and no properties with the value undefined:\n";
        return prompt;
    }

    static PromptSection IntroSection(string typeName, string schema)
    {
        PromptSection introSection = new PromptSection();
        introSection += $"You are a service that translates user requests into JSON objects of type \"{typeName}\" according to the following TypeScript definitions:\n";
        introSection += $"###\n{schema}###\n";
        return introSection;
    }

    public static PromptSection RequestSection(string request)
    {
        PromptSection requestSection = new PromptSection();
        requestSection += "The following is a user request:\n";
        requestSection += $"\"\"\"\n{request}\n\"\"\"\n";
        requestSection += "The following is the user request translated into a JSON object with 2 spaces of indentation and no properties with the value undefined:\n";
        return requestSection;
    }

    public static string RepairPrompt(string validationError)
    {
        validationError ??= string.Empty;
        return "The JSON object is invalid for the following reason:\n" +
               $"{validationError}\n" +
               "The following is a revised JSON object. Do not include explanations.\n";
    }
}
