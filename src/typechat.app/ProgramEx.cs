// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public static class ProgramEx
{
    public static bool PrintNotTranslated(this Program program)
    {
        if (program.HasNotTranslated)
        {
            Console.WriteLine("I could not translate the following:");
            ConsoleApp.WriteLines(program.NotTranslated);
            Console.WriteLine();
            return true;
        }

        return false;
    }

    public static bool PrintTranslationNotes(this Program program)
    {
        if (!string.IsNullOrEmpty(program.TranslationNotes))
        {
            Console.WriteLine("Translation Notes:");
            Console.WriteLine(program.TranslationNotes);
            Console.WriteLine();
            return true;
        }

        return false;
    }
}
