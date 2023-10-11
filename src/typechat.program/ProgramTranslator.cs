// Copyright (c) Microsoft. All rights reserved.

using Microsoft.TypeChat.Validation;

namespace Microsoft.TypeChat;

/// <summary>
/// A ProgramTranslator translates natural language requests into type safe programs that can
/// call methods on a target API. The program can then be verified - type checked - for safety.
/// Errors can be passed back to the model, which can use them to repair the program.
/// Program are run executed using an interprter or compiled with a ProgramCompiler into
/// type safe lambdas
/// </summary>
public class ProgramTranslator : JsonTranslator<Program>
{
    internal const string SchemaText =
@"
// A program consists of a sequence of function calls that are evaluated in order.
export type Program = {
    ""@steps"": FunctionCall[];
    // Collect parts of the user request that could not translated.
    // Including if NO suitable API provided
    ""@cannot_translate""?: string[];
}

// A function call specifies a function name and a list of argument expressions. Arguments may contain
// nested function calls and result references.
export type FunctionCall = {
    // Name of the function
    ""@func"": string;
    // Arguments for the function, if any
    ""@args""?: Expression[];
};

// An expression is a JSON value, a function call, or a reference to the result of a preceding expression.
export type Expression = JsonValue | FunctionCall | ResultReference;

// A JSON value is a string, a number, a boolean, null, an object, or an array. Function calls and result
// references can be nested in objects and arrays.
export type JsonValue = string | number | boolean | null | { [x: string]: Expression } | Expression[];

// A result reference represents the value of an expression from a preceding step.
export type ResultReference = {
    // Index of the previous expression in the ""@steps"" array
    ""@ref"": number;
};
";
    internal static readonly TypescriptSchema ProgramSchema;

    static ProgramTranslator()
    {
        ProgramSchema = GetProgramSchema();
    }
    /// <summary>
    /// Return the schema used for program synthesis
    /// The schema is currently written by hand in Typescript and is identical to the one
    /// used in Typechat
    /// </summary>
    /// <returns></returns>
    internal static TypescriptSchema GetProgramSchema()
    {
        return new TypescriptSchema(typeof(Program), SchemaText);
    }

    SchemaText _apiDef;

    /// <summary>
    /// Create a program translator that uses the given language model to create programs
    /// that can call the given API. The API is the interface or class definition, whose methods
    /// and method signatures define the API surface. 
    /// </summary>
    /// <param name="model">language model</param>
    /// <param name="validator">Validator that verifies the returned program</param>
    /// <param name="apiDef">API definition</param>
    public ProgramTranslator(ILanguageModel model, IJsonTypeValidator<Program> validator, SchemaText apiDef)
        : base(
            model,
            validator,
            new ProgramTranslatorPrompts(apiDef)
            )
    {
        _apiDef = apiDef;
    }

    /// <summary>
    /// Api definition
    /// </summary>
    public SchemaText ApiDef => _apiDef;

    // return true if validation loop should continue
    protected override bool OnValidationComplete(Result<Program> validationResult)
    {
        // If LLM could not translate the user request to begin with, then we can't continue
        Program? program = validationResult.Value;
        return (program != null) ? program.HasNotTranslated : true;
    }
}

/// <summary>
/// A ProgramTranslator translates natural language requests into type safe programs that call methods on
/// the provided API of type TApi
/// The Program can be verified - type checked. Errors can be passed back to the model, which can use them to
/// repair the returned program.
/// Program are run using a ProgramInterprter or compiled with a ProgramCompiler into type safe lambdas
/// </summary>
/// <typeparam name="TApi">Api definition</typeparam>
public class ProgramTranslator<TApi> : ProgramTranslator
{
    /// <summary>
    /// Create a ProgramTranslator that uses the given model to synthesize programs that can
    /// call the provided Api
    /// </summary>
    /// <param name="model"></param>
    /// <param name="api"></param>
    public ProgramTranslator(ILanguageModel model, Api<TApi> api)
        : base(
            model,
            new ProgramValidator<TApi>(api),
            api.GenerateSchema().Schema
        )
    {
    }
}
