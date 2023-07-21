// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public static class Prompts
{
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
               $"\"\n{request}\n\"\n" +
               "The following is the user request translated into a JSON object with 2 spaces of indentation and no properties with the value undefined:\n";
    }

    public static string RepairPrompt(string json, TypeSchema schema, string validationError)
    {
        return  $"The following is an INVALID JSON object of type \"{schema.TypeName}\"\n" +
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
