// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// Translates natural language requests into simple programs, represented as JSON, that compose
/// </summary>
public class ProgramTranslator : JsonTranslator<Program>
{
    public ProgramTranslator(ILanguageModel model, Type type)
        : this(model, TypescriptExporter.GenerateAPI(type))
    {
    }

    public ProgramTranslator(ILanguageModel model, TypeSchema schema)
        : this(model, schema.Schema)
    {
    }

    public ProgramTranslator(ILanguageModel model, string apiDef)
        : this(
              model,
              TypescriptSchema.Load(typeof(Program), "ProgramSchema.ts"),
              apiDef
              )
    {
    }

    public ProgramTranslator(ILanguageModel model, TypescriptSchema schema, string apiDef)
        : base(
            model,
            new TypeValidator<Program>(schema),
            new ProgramTranslatorPrompts(apiDef)
            )
    {
    }

    public static ProgramTranslator Create(ILanguageModel model, string apiDef)
    {
        TypescriptSchema schema = TypescriptExporter.GenerateSchema(typeof(Program));
        return new ProgramTranslator(model, schema, apiDef);
    }
}
