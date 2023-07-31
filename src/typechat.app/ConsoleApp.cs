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


    public string? ConsolePrompt { get; set; } = ">";
    public IList<string> StopStrings => _stopStrings;
    public string CommentPrefix = "#";

    public Task RunAsync(string consolePrompt, string? inputFilePath = null)
    {
        ConsolePrompt = consolePrompt;
        if (string.IsNullOrEmpty(inputFilePath))
        {
            return RunAsync();
        }
        else
        {
            return RunBatchAsync(inputFilePath);
        }
    }

    public async Task RunAsync(CancellationToken cancelToken = default)
    {
        while (!cancelToken.IsCancellationRequested)
        {
            Console.Write(ConsolePrompt);
            string? input = await ReadLineAsync(cancelToken).ConfigureAwait(false);
            input = input.Trim();
            if (string.IsNullOrEmpty(input))
            {
                continue;
            }
            if (IsStop(input))
            {
                break;
            }
            await EvalInput(input, cancelToken).ConfigureAwait(false);
        }
    }

    public async Task RunBatchAsync(string batchFilePath, CancellationToken cancelToken = default)
    {
        using var reader = new StreamReader(batchFilePath);
        string line = null;
        while (!cancelToken.IsCancellationRequested &&
              (line = reader.ReadLine()) != null)
        {
            line = line.Trim();
            if (line.Length == 0 ||
               line.StartsWith(CommentPrefix))
            {
                continue;
            }

            Console.Write(ConsolePrompt);
            Console.WriteLine(line);
            await EvalInput(line, cancelToken).ConfigureAwait(false);
        }
    }

    async Task EvalInput(string input, CancellationToken cancelToken)
    {
        try
        {
            await ProcessRequestAsync(input, cancelToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await OnError(input, ex);
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

    protected abstract Task ProcessRequestAsync(string input, CancellationToken cancelToken);

    protected void SubscribeAllEvents<T>(JsonTranslator<T> translator)
    {
        translator.SendingPrompt += this.OnSendingPrompt;
        translator.AttemptingRepair += this.OnAttemptingRepairs;
        translator.CompletionReceived += this.OnCompletionReceived;
    }

    protected virtual Task OnError(string input, Exception ex)
    {
        Console.WriteLine(ex);
        Console.WriteLine();
        return Task.CompletedTask;
    }

    protected void OnSendingPrompt(string value)
    {
        Console.WriteLine("### PROMPT ");
        Console.WriteLine(value);
        Console.WriteLine("###");
    }

    protected void OnCompletionReceived(string value)
    {
        Console.WriteLine("### COMPLETION ");
        Console.WriteLine(value);
        Console.WriteLine("###");
    }

    protected void OnAttemptingRepairs(string value)
    {
        Console.WriteLine("### REPAIRING ERROR: ");
        Console.WriteLine(value);
        Console.WriteLine("###");
    }
}
