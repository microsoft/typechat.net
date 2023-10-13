// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// Given a structurally valid Program, this ensures that the Program can
/// correctly bind to its target API: e.g calls are being made to functions that exist..
/// </summary>
public interface IProgramValidator
{
    Result<Program> ValidateProgram(Program program);
}

/// <summary>
/// Transforms a raw JSON object into a validated, typed, Program
/// </summary>
public class ProgramValidator : IJsonTypeValidator<Program>, IProgramValidator
{
    private TypeValidator<Program> _typeValidator;
    private IProgramValidator? _programValidator;

    public ProgramValidator(IProgramValidator? programValidator = null)
    {
        _typeValidator = new TypeValidator<Program>(ProgramTranslator.ProgramSchema);
        _programValidator = programValidator;
    }

    /// <summary>
    /// Program Schema
    /// </summary>
    public TypeSchema Schema => _typeValidator.Schema;

    public Result<Program> Validate(string json)
    {
        // First, validate the program json
        Result<Program> result = _typeValidator.Validate(json);
        if (result.Success)
        {
            // If parts of the user's request could not be translated, stop
            if (!result.Value.HasNotTranslated)
            {
                // Now validate the full program
                return ValidateProgram(result.Value);
            }
        }

        return result;
    }

    public virtual Result<Program> ValidateProgram(Program program)
    {
        if (_programValidator != null)
        {
            return _programValidator.ValidateProgram(program);
        }

        return program;
    }
}

/// <summary>
/// Compiles the program targeting an Api of type TApi
/// Any compilation errors are sent to the LLM for correcting the program.
/// </summary>
/// <typeparam name="TApi"></typeparam>
public class ProgramValidator<TApi> : ProgramValidator
{
    private Api<TApi> _api;

    public ProgramValidator(Api<TApi> api)
        : base()
    {
        _api = api;
    }

    /// <summary>
    /// Default Compiler: Compiles into a Linq Expression Tree, type checking in the process
    /// </summary>
    public override Result<Program> ValidateProgram(Program program)
    {
        ProgramCompiler compiler = new ProgramCompiler(_api.TypeInfo);
        try
        {
            var lambdaExpr = compiler.CompileToExpressionTree(program, _api.Implementation);
            program.Delegate = lambdaExpr.Compile();
            return new Result<Program>(program);
        }
        catch (Exception ex)
        {
            return Result<Program>.Error(ex.Message);
        }
    }
}
