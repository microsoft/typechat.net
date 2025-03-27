// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// Default Prompts used by the program translator
/// </summary>
public class ProgramTranslatorPrompts : JsonTranslatorPrompts
{
    private string _apiDef;

    /// <summary>
    /// Create a program translator
    /// The apiDef string contains the methods and their signatures 
    /// </summary>
    /// <param name="apiDef"></param>
    public ProgramTranslatorPrompts(string apiDef)
    {
        ArgumentVerify.ThrowIfNullOrEmpty(apiDef, nameof(apiDef));
        _apiDef = apiDef;
    }

    public override Prompt CreateRequestPrompt(TypeSchema schema, Prompt request, IList<IPromptSection> context)
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
        AddContextAndRequest(prompt, request, context);
        return prompt;
    }
}
