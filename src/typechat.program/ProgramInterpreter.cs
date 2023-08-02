// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public class ProgramInterpreter
{
    List<object> _results;

    public ProgramInterpreter()
    {
        _results = new List<object>();
    }

    public object? Run(Program program, Func<Call, object?> caller)
    {
        ArgumentNullException.ThrowIfNull(program, nameof(program));
        _results.Clear();

        Steps steps = program.Steps;
        for (int i = 0; i < steps.Calls.Length; ++i)
        {
            object? result = caller(steps.Calls[i]);
            _results.Add(result);
        }
        return (_results.Count > 0) ? _results[_results.Count - 1] : null;
    }
}
