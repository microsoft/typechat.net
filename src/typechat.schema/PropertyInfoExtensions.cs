// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Schema;

internal static class PropertyInfoExtensions
{
    internal static bool IsAbstract(this PropertyInfo property)
    {
        var methods = property.GetAccessors();
        for (int i = 0; i < methods.Length; ++i)
        {
            if (methods[i].IsAbstract)
            {
                return true;
            }
        }
        return false;
    }
}
