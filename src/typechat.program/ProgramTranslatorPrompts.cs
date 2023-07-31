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

    public override string CreateRequestPrompt(TypeSchema schema, string request)
    {
        return RequestPrompt(request, schema.Schema.Text, _apiDef);
    }

    public static string RequestPrompt(string request, string programSchema, string apiDef)
    {
        return "You are a service that translates user requests into programs represented as JSON using the following TypeScript definitions:\n" +
               $"###\n{programSchema}###\n" +
               "The programs can call functions from the API defined in the following TypeScript definitions:\n" +
               $"###\n{apiDef}###\n" +
               "The following is a user request:\n" +
               $"\"\"\"\n{request}\n\"\"\"\n" +
               "The following is the user request translated into a JSON object with NO indentation and no properties with the value undefined:\n";
    }
}
