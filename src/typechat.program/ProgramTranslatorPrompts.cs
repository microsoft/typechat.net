// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public class ProgramTranslatorPrompts : JsonTranslatorPrompts
{
    string _apiDef;

    public ProgramTranslatorPrompts(string apiDef)
    {
        ArgumentException.ThrowIfNullOrEmpty(apiDef, nameof(apiDef));
        _apiDef = apiDef;
    }

    public override Prompt CreateRequestPrompt(TypeSchema schema, string request, IList<IPromptSection> context)
    {
        return RequestProgramPrompt(request, schema.Schema.Text, _apiDef, context);
    }

    public static Prompt RequestProgramPrompt(string request, string programSchema, string apiDef, IList<IPromptSection> context)
    {
        Prompt prompt = new Prompt();
        prompt += "You are a service that translates user requests into programs represented as JSON using the following TypeScript definitions:\n" +
               $"```\n{programSchema}```\n" +
               "The programs can call functions from the API defined in the following TypeScript definitions:\n" +
               $"```\n{apiDef}```\n";
        AddContext(prompt, context);
        prompt += RequestSection(request);
        return prompt;
    }
}
