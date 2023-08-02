// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Tests;

public class StringAPI
{
    public AnyJsonValue HandleCall(string name, AnyJsonValue[] args)
    {
        switch(name)
        {
            default:
                throw new NotSupportedException(name);
            case "concat":
                break;
        }

        return AnyJsonValue.Undefined;
    }
}
