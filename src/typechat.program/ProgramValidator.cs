// Copyright (c) Microsoft. All rights reserved.

using System.Linq.Expressions;

namespace Microsoft.TypeChat;

public interface IProgramValidator
{
    ValidationResult<Program> ValidateProgram(Program program);
}

public class ProgramValidator : IJsonTypeValidator<Program>
{
    TypeValidator<Program> _typeValidator;
    IProgramValidator? _programValidator;

    public ProgramValidator(IProgramValidator? programValidator = null)
    {
        _typeValidator = new TypeValidator<Program>(ProgramTranslator.ProgramSchema);
        _programValidator = programValidator;
    }

    public TypeSchema Schema => _typeValidator.Schema;

    public ValidationResult<Program> Validate(string json)
    {
        // First, validate the program json
        ValidationResult<Program> result = _typeValidator.Validate(json);
        if (result.Success)
        {
            // Now validate the actual parsed program
            return ValidateProgram(result.Value);
        }
        return result;
    }

    public virtual ValidationResult<Program> ValidateProgram(Program program)
    {
        // Now validate the actual parsed program
        if (_programValidator != null)
        {
            return _programValidator.ValidateProgram(program);
        }
        return program;
    }
}

/// <summary>
/// Compiles the program targeting TApi
/// Any compilation errors can be used for correcting the program.
/// </summary>
/// <typeparam name="TApi"></typeparam>
public class ProgramValidator<TApi> : ProgramValidator, IProgramValidator
{
    Api<TApi> _api;

    public ProgramValidator(Api<TApi> api)
        : base()
    {
        _api = api;
    }

    /// <summary>
    /// Default Compiler: Compiles into a Linq Expression Tree, type checking in the process
    /// </summary>
    public override ValidationResult<Program> ValidateProgram(Program program)
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
