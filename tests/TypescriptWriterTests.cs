// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Tests;

public class TypescriptWriterTests : TypeChatTest
{
    [Fact]
    public void TestMethodDeclare()
    {
        string code = TypescriptWriter.WriteCode((writer) =>
        {
            writer.BeginMethodDeclare("add");
            {
                writer.Argument("x", Typescript.Types.Number, 0, 2)
                      .Argument("y", Typescript.Types.Number, 1, 2);
            }
            writer.EndMethodDeclare(Typescript.Types.Number);
        });
        ValidateContains(code, "x", Typescript.Types.Number);
        ValidateContains(code, "y", Typescript.Types.Number);
        ValidateContains(code, $": {Typescript.Types.Number}");
    }

}
