// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.CSharp;

/// <summary>
/// An in-memory assembly containing compiled code that can be executed
/// </summary>
public class ProgramAssembly
{
    private readonly Assembly _assembly;
    private readonly string _className;
    private readonly string _methodName;

    public ProgramAssembly(byte[] bytes, CSharpProgramTranspiler writer)
        : this(bytes, writer.ClassName, writer.MethodName)
    {
    }

    public ProgramAssembly(byte[] bytes, string className, string methodName)
    {
        ArgumentVerify.ThrowIfNull(bytes, nameof(bytes));
        ArgumentVerify.ThrowIfNullOrEmpty(className, nameof(className));
        ArgumentVerify.ThrowIfNullOrEmpty(methodName, nameof(methodName));

        _assembly = Assembly.Load(bytes);
        _className = className;
        _methodName = methodName;
    }

    public dynamic Run(object api)
    {
        object instance = _assembly.CreateInstance(_className);
        if (instance is null)
        {
            throw new InvalidOperationException();
        }
        dynamic retVal = instance.GetType().InvokeMember(
            _methodName,
            BindingFlags.InvokeMethod,
            null,
            instance,
            new object[] { api }
        );
        return retVal;
    }
}
