// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// Translates natural language requests into simple programs, represented as JSON, that compose
/// </summary>
public class ProgramTranslator : JsonTranslator<Program>
{
    internal static readonly TypescriptSchema ProgramSchema;

    static ProgramTranslator()
    {
        ProgramSchema = GetProgramSchema();
    }

    public static TypescriptSchema GetProgramSchema()
    {
        return TypescriptSchema.Load(typeof(Program), "ProgramSchema.ts");
    }

    string _apiDef;

    public ProgramTranslator(ILanguageModel model, TypeSchema apiSchema)
        : this(model, apiSchema.Schema)
    {
    }

    public ProgramTranslator(ILanguageModel model, string apiDef)
        : this(
              model,
              new TypeValidator<Program>(ProgramSchema),
              apiDef
              )
    {
    }

    public ProgramTranslator(ILanguageModel model, IJsonTypeValidator<Program> validator, string apiDef)
        : base(
            model,
            validator,
            new ProgramTranslatorPrompts(apiDef)
            )
    {
        _apiDef = apiDef;
    }

    public string ApiDef => _apiDef;

    protected override Result<Program> OnEmptyResponse(JsonResponse response)
    {
        string message = response.Message();
        Program emptyProgram = new Program();
        emptyProgram.TranslationNotes = message;
        return Result<Program>.Error(emptyProgram, message);
    }

    // Return true if validation loop should continue
    protected override bool OnValidationComplete(JsonResponse response, Result<Program> validationResult)
    {
        Program? program = validationResult.Value;
        if (program != null)
        {
            // We did parse a program, but it may not be valid
            if (!program.IsComplete)
            {
                // Try to gather whatever explanation the LLM returned
                program.TranslationNotes = response.Message();
                // If the AI returned elements it could not translate  to begin with, just stop
                return !program.HasNotTranslated;
            }
        }
        return true;
    }
}

public class ProgramTranslator<TApi> : ProgramTranslator
{
    public ProgramTranslator(ILanguageModel model, Api<TApi> api)
        : base(
            model,
            new ProgramValidator<TApi>(api),
            api.GenerateSchema().Schema
        )
    {
    }
}
