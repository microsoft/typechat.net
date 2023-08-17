// Copyright (c) Microsoft. All rights reserved.

using System.Linq.Expressions;

namespace Microsoft.TypeChat;

/// <summary>
/// Compiles the program targeting TApi
/// Any compilation errors can be used for correcting the program.
/// </summary>
/// <typeparam name="TApi"></typeparam>
public class ProgramValidator<TApi> : IJsonTypeValidator<Program>
{
    Api<TApi> _api;
    TypeValidator<Program> _typeValidator;
    Func<Program, ValidationResult<Program>> _compiler;

    public ProgramValidator(Api<TApi> api, Func<Program, ValidationResult<Program>> compiler = null)
    {
        _api = api;
        _typeValidator = new TypeValidator<Program>(ProgramTranslator.ProgramSchema);
        compiler ??= this.Compile;
        _compiler = compiler;
    }

    public TypeSchema Schema => _typeValidator.Schema;

    public ValidationResult<Program> Validate(string json)
    {
        // First, validate the program json
        ValidationResult<Program> result = _typeValidator.Validate(json);
        if (result.Success)
        {
            // Now validate the actual parsed program
            return _compiler(result);
        }
        return result;
    }

    /// <summary>
    /// Default Compiler: Compiles into a Linq Expression Tree, type checking in the process
    /// </summary>
    public ValidationResult<Program> Compile(Program program)
    {
        ProgramCompiler compiler = new ProgramCompiler(_api.TypeInfo);
        try
        {
            var lambdaExpr = compiler.CompileToExpressionTree(program, _api.Implementation);
            program.Delegate = lambdaExpr.Compile();
            return new ValidationResult<Program>(program);
        }
        catch (Exception ex)
        {
            return ValidationResult<Program>.Error(ex.Message);
        }
    }
}
