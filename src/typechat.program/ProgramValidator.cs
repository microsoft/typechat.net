// Copyright (c) Microsoft. All rights reserved.

using System.Linq.Expressions;

namespace Microsoft.TypeChat;

/// <summary>
/// Compiles the program targeting TApi into a Linq Expression Tree.
/// Any compilation errors can be used for correcting the program.
/// </summary>
/// <typeparam name="TApi"></typeparam>
public class ProgramValidator<TApi> : IJsonTypeValidator<Program>
{
    Api<TApi> _api;
    TypeValidator<Program> _typeValidator;
    ProgramCompiler _compiler;

    public ProgramValidator(Api<TApi> api)
    {
        _api = api;
        _typeValidator = new TypeValidator<Program>(ProgramTranslator.ProgramSchema);
        _compiler = new ProgramCompiler(api.TypeInfo);
    }

    public TypeSchema Schema => _typeValidator.Schema;

    public ValidationResult<Program> Validate(string json)
    {
        // First, validate the program json
        ValidationResult<Program> result = _typeValidator.Validate(json);
        if (result.Success)
        {
            // Now validate the actual parsed program
            return Validate(result);
        }
        return result;
    }

    public ValidationResult<Program> Validate(Program program)
    {
        try
        {
            program.Lambda = _compiler.CompileToExpressionTree(program, _api.Implementation);
            return new ValidationResult<Program>(program);
        }
        catch (Exception ex)
        {
            return ValidationResult<Program>.Error(ex.Message);
        }
    }
}
