// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// The legacy full-request prompts. Supply these explicitly for the old layout, or
/// derive from <see cref="SchemaFirstJsonTranslatorPrompts"/> to customize system instructions.
/// </summary>
public class JsonTranslatorPrompts : IJsonTranslatorPrompts
{
    internal static readonly JsonTranslatorPrompts Default = new SchemaFirstJsonTranslatorPrompts();

    /// <summary>Opt into the original wrapped request layout, including for text-only models.</summary>
    public static JsonTranslatorPrompts Legacy { get; } = new JsonTranslatorPrompts();

    public virtual Prompt CreateRequestPrompt(TypeSchema typeSchema, Prompt request, IList<IPromptSection> context = null)
    {
        ArgumentVerify.ThrowIfNull(request, nameof(request));
        Prompt prompt = new Prompt { AllowTextFlattening = true };

        prompt += IntroSection(typeSchema.TypeFullName, typeSchema.Schema);
        AddContextAndRequest(prompt, request, context);

        return prompt;
    }

    public virtual string CreateRepairPrompt(TypeSchema schema, string json, string validationError)
    {
        return RepairPrompt(validationError);
    }

    /// <summary>
    /// Add the given user request and any context to the prompt we are sending to the model
    /// </summary>
    /// <param name="prompt">prompt being constructed</param>
    /// <param name="request">user request</param>
    /// <param name="context">any RAG context</param>
    /// <returns>prompt to send to the model</returns>
    public static Prompt AddContextAndRequest(Prompt prompt, Prompt request, IList<IPromptSection> context)
    {
        if (!context.IsNullOrEmpty())
        {
            prompt.Append(context);
        }

        if (IsSingleUserText(request))
        {
            prompt += RequestSection(request[0].GetText());
            return prompt;
        }

        prompt.AppendInstruction("USER REQUEST:");
        prompt.Append(request);
        prompt += "The following is USER REQUEST translated into a JSON object with 2 spaces of indentation and no properties with the value undefined:\n";
        return prompt;
    }

    /// <summary>Only an ordinary, single user text section can be wrapped without losing content or roles.</summary>
    protected static bool IsSingleUserText(Prompt request) =>
        request.Count == 1 && request[0].GetType() == typeof(PromptSection) && request[0].Source == PromptSection.Sources.User;

    /// <summary>
    /// Adds a section that tells the model that its task to is translate requests into JSON matching the
    /// given schema
    /// </summary>
    /// <param name="typeName"></param>
    /// <param name="schema"></param>
    /// <returns></returns>
    public static PromptSection IntroSection(string typeName, string schema)
    {
        return PromptSection.FromUser(SchemaInstructions(typeName, schema));
    }

    internal static string SchemaInstructions(string typeName, string schema) =>
        $"You are a service that translates user requests into JSON objects of type \"{typeName}\" according to the following TypeScript definitions:\n" +
        $"###\n{schema}###\n";

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
