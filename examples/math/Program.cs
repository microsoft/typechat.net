// Copyright (c) Microsoft. All rights reserved.
using Microsoft.TypeChat;

namespace Math;

public class Math : ConsoleApp
{
    Math()
    {
        // Uncomment to see ALL raw messages to and from the AI
        //base.SubscribeAllEvents(_service);
    }

    protected override Task ProcessRequestAsync(string input, CancellationToken cancelToken)
    {
        return Task.CompletedTask;
    }

    public static async Task<int> Main(string[] args)
    {
        try
        {
            Math app = new Math();
            // Un-comment to print auto-generated schema at start:
            // Console.WriteLine(app.Schema.Schema.Text);
            await app.RunAsync("➕➖✖️➗🟰> ", args.GetOrNull(0));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return -1;
        }

        return 0;
    }
}
