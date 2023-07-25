// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public abstract class ConsoleApp
{
    List<string> _stopStrings;

    public ConsoleApp()
    {
        Console.OutputEncoding = Encoding.UTF8;
        _stopStrings = new List<string>(2) { "quit", "exit" };
    }


    public string? InteractivePrompt { get; set; } = "😀> ";
    public IList<string> StopStrings => _stopStrings;

    public async Task RunAsync(CancellationToken cancelToken = default)
    {
        while (!cancelToken.IsCancellationRequested)
        {
            Console.Write(InteractivePrompt);
            string? input = await ReadLineAsync(cancelToken).ConfigureAwait(false);
            if (IsStop(input))
            {
                break;
            }
            try
            {
                await ProcessRequestAsync(input, cancelToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await WriteLineAsync(ex.Message).ConfigureAwait(false);
            }
        }
    }

    bool IsStop(string? line)
    {
        if (line == null)
        {
            return true;
        }
        return _stopStrings.Contains(line, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<string?> ReadLineAsync(CancellationToken cancelToken)
    {
        string? line = await Console.In.ReadLineAsync(cancelToken).ConfigureAwait(false);
        return (line != null) ? line.Trim() : line;
    }

    public async Task WriteLineAsync(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            Console.Out.WriteLine();
        }
        else
        {
            await Console.Out.WriteLineAsync(value).ConfigureAwait(false);
        }
    }

    protected abstract Task ProcessRequestAsync(string line, CancellationToken cancelToken);
}
