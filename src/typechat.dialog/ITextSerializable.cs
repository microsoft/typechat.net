// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Dialog;

/// <summary>
/// Objects can implement this interface to customize how they are transformed into strings
/// Which is different from ToString()
/// </summary>
public interface ITextSerializable
{
    /// <summary>
    /// Turn this object into a string. The string output may be differen from what ToString() produces
    /// </summary>
    /// <returns>string</returns>
    string Stringify();
}
