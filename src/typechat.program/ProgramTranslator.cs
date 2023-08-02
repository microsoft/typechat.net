// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public class ProgramTranslator : JsonTranslator<Program>
{
    public ProgramTranslator(ILanguageModel model, string apiDef)
        : this(
              model,
              //TypescriptExporter.GenerateSchema(typeof(Program)),
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
