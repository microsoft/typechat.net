// Copyright (c) Microsoft. All rights reserved.
using Microsoft.TypeChat;
using Microsoft.TypeChat.Schema;

namespace Library;


public class LibraryApp : ConsoleApp
{
    JsonTranslator<Book> _translator;

    public LibraryApp()
    {
        _translator = new JsonTranslator<Book>(
            new LanguageModel(Config.LoadOpenAI()),
            new TypeValidator<Book>()
        );

        // Uncomment to see ALL raw messages to and from the AI
        //base.SubscribeAllEvents(_translator);
    }
    public TypeSchema Schema => _translator.Validator.Schema;

    public override async Task ProcessRequestAsync(string input, CancellationToken cancelToken)
    {
        Book book = await _translator.TranslateAsync(input);

        string json = Json.Stringify(book);
        Console.WriteLine(json);

        if (!book.Title.Any() || book is UnknownBook)
        {
            Console.WriteLine("Unable to provide a good book :(");
        }
        else
        {
            Console.WriteLine("Success!");
        }
    }

    public static async Task<int> Main(string[] args)
    {
        try
        {
            LibraryApp app = new LibraryApp();
            // Un-comment to print auto-generated schema at start:
            //Console.WriteLine(app.Schema.Schema.Text);

            await app.RunAsync("☕> ", args.GetOrNull(0));
        }
        catch (Exception ex)
        {
            WriteError(ex);
            return -1;
        }

        return 0;
    }
}
