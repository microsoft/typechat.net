// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public static class ProgramExtensions
{
    /// <summary>
    /// Get the array item at position 'index'. If index is beyond the length of args
    /// or args is null, return null
    /// </summary>
    /// <param name="array">array</param>
    /// <param name="index">position to retrieve item at</param>
    /// <returns>string or null</returns>
    public static string? GetOrNull(this string[] array, int index)
    {
        if (array == null ||
            index >= array.Length)
        {
            return null;
        }
        return array[index];
    }

    public static void Print(this TypeChatException ex)
    {
        Console.WriteLine($"## TypeChatException");
        Console.WriteLine(ex.ToString());
    }

    public static void PrintNotTranslated(this Program program)
    {
        if (program != null && program.HasNotTranslated)
        {
            Console.WriteLine("I could not translate the following:");
            ConsoleApp.WriteLines(program.NotTranslated);
            Console.WriteLine();
        }
    }

    /// <summary>
    /// Print the given program as pseudo C# like code
    /// </summary>
    /// <param name="program"></param>
    /// <param name="apiType"></param>
    public static void Print(this Program program, string apiType)
    {
        if (program == null)
        {
            return;
        }
        program.PrintNotTranslated();
        if (program.HasSteps && program.HasNotTranslated)
        {
            Console.WriteLine("Suggested program that may include suggested APIs:");
        }
        new ProgramWriter(Console.Out).Write(program, apiType);
    }

    internal static void WaitForResult(this Task task)
    {
        task.ConfigureAwait(false).GetAwaiter().GetResult();
    }

    internal static T WaitForResult<T>(this Task<T> task)
    {
        return task.ConfigureAwait(false).GetAwaiter().GetResult();
    }
}
