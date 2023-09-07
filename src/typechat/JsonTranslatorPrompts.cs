// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public class JsonTranslatorPrompts : IJsonTranslatorPrompts
{
    public static readonly JsonTranslatorPrompts Default = new JsonTranslatorPrompts();

    public virtual Prompt CreateRequestPrompt(TypeSchema schema, string request, IList<IPromptSection> preamble = null)
    {
        //return RequestPrompt(schema, request);
        return RequestPrompt(schema.TypeFullName, schema.Schema, request, preamble);
    }

    public virtual string CreateRepairPrompt(TypeSchema schema, string json, string validationError)
    {
        return RepairPrompt(validationError);
    }

    public static string RequestPrompt(TypeSchema schema, string request)
    {
        ArgumentNullException.ThrowIfNull(schema, nameof(schema));
        return RequestPrompt(schema.TypeName, schema.Schema, request);
    }

    public static string RequestPrompt(string typeName, string schema, string request)
    {
        return $"You are a service that translates user requests into JSON objects of type \"{typeName}\" according to the following TypeScript definitions:\n" +
               $"###\n{schema}###\n" +
               "The following is a user request:\n" +
               $"\"\"\"\n{request}\n\"\"\"\n" +
               "The following is the user request translated into a JSON object with 2 spaces of indentation and no properties with the value undefined:\n";
    }

    public static Prompt RequestPrompt(string typeName, string schema, string request, IList<IPromptSection> context)
    {
        Prompt prompt = new Prompt();

        prompt += IntroSection(typeName, schema);
        AddContext(prompt, context);
        prompt += RequestSection(request);

        return prompt;
    }

    public static PromptSection IntroSection(string typeName, string schema)
    {
        PromptSection introSection = new PromptSection();
        introSection += $"You are a service that translates user requests into JSON objects of type \"{typeName}\" according to the following TypeScript definitions:\n";
        introSection += $"###\n{schema}###\n";
        return introSection;
    }

    public static Prompt AddContext(Prompt prompt, IList<IPromptSection> context)
    {
        if (!context.IsNullOrEmpty())
        {
            prompt += "The following is history and context PERTINENT to the user request:";
            prompt += context;
        }
        return prompt;
    }

    public static PromptSection RequestSection(string request)
    {
        PromptSection requestSection = new PromptSection();
        requestSection += "The following is a user request:\n";
        requestSection += $"\"\"\"\n{request}\n\"\"\"\n";
        requestSection += "The following is the user request translated into a JSON object with 2 spaces of indentation and no properties with the value undefined:\n";
        return requestSection;
    }

    public static string RepairPrompt(string json, TypeSchema schema, string validationError)
    {
        return $"The following is an INVALID JSON object of type \"{schema.TypeName}\"\n" +
                $"###\n{json}###\n" +
                "The JSON should match this Typescript definition:" +
                $"###\n{schema.Schema.Text}###\n" +
                RepairPrompt(validationError);
    }

    public static string RepairPrompt(string validationError)
    {
        validationError ??= string.Empty;
        return "The JSON object is invalid for the following reason:\n" +
               $"{validationError}\n" +
               "The following is a revised JSON object. Do not include explanations.\n";
    }
}
