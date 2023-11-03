// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public abstract class ConsoleApp : IInputHandler
{
    private List<string> _stopStrings;

    public ConsoleApp()
    {
        Console.OutputEncoding = Encoding.UTF8;
        _stopStrings = new List<string>(2) { "quit", "exit" };
    }

    public string? ConsolePrompt { get; set; } = ">";

    public IList<string> StopStrings => _stopStrings;

    public string CommentPrefix { get; set; } = "#";

    public string CommandPrefix { get; set; } = "@";

    public async Task RunAsync(string consolePrompt, string? inputFilePath = null, CancellationToken cancelToken = default)
    {
        ConsolePrompt = consolePrompt;
        await InitAppAsync(cancelToken).ConfigureAwait(false);
        if (string.IsNullOrEmpty(inputFilePath))
        {
            await RunAsync(cancelToken).ConfigureAwait(false);
        }
        else
        {
            await RunBatchAsync(inputFilePath, cancelToken).ConfigureAwait(false);
        }
    }

    public async Task RunAsync(CancellationToken cancelToken = default)
    {
        while (!cancelToken.IsCancellationRequested)
        {
            Console.Write(ConsolePrompt);
            string? input = await ReadLineAsync(cancelToken).ConfigureAwait(false);
            input = input.Trim();
            if (!string.IsNullOrEmpty(input) &&
                !await EvalInputAsync(input, cancelToken).ConfigureAwait(false))
            {
                break;
            }
        }
    }

    public async Task RunBatchAsync(string batchFilePath, CancellationToken cancelToken = default)
    {
        using var reader = new StreamReader(batchFilePath);
        string line = null;
        while (!cancelToken.IsCancellationRequested &&
              (line = reader.ReadLine()) is not null)
        {
            line = line.Trim();
            if (line.Length == 0 ||
               line.StartsWith(CommentPrefix))
            {
                continue;
            }

            Console.Write(ConsolePrompt);
            Console.WriteLine(line);
            await EvalInputAsync(line, cancelToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Return false if should exit
    /// </summary>
    private async Task<bool> EvalInputAsync(string input, CancellationToken cancelToken)
    {
        try
        {
            return input.StartsWith(CommandPrefix)
                ? await EvalCommandAsync(input, cancelToken).ConfigureAwait(false)
                : await EvalLineAsync(input, cancelToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            OnException(input, ex);
        }

        return true;
    }

    private async Task<bool> EvalLineAsync(string input, CancellationToken cancelToken)
    {
        if (IsStop(input))
        {
            return false;
        }

        await ProcessInputAsync(input, cancelToken).ConfigureAwait(false);

        return true;
    }

    private async Task<bool> EvalCommandAsync(string input, CancellationToken cancelToken)
    {
        // Process a command
        List<string> parts = CommandLineStringSplitter.Instance.Split(input).ToList();
        if (parts.IsNullOrEmpty())
        {
            return true;
        }

        string cmd = parts[0].Substring(CommandPrefix.Length);
        if (!string.IsNullOrEmpty(cmd))
        {
            if (IsStop(cmd))
            {
                return false;
            }

            parts.RemoveAt(0);
            await ProcessCommandAsync(cmd, parts).ConfigureAwait(false);
        }

        return true;
    }

    private bool IsStop(string? line)
    {
        return line is null || _stopStrings.Contains(line, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<string?> ReadLineAsync(CancellationToken cancelToken = default)
    {
#if NET7_0_OR_GREATER
        string? line = await Console.In.ReadLineAsync(cancelToken).ConfigureAwait(false);
#else
        string? line = await Console.In.ReadLineAsync().ConfigureAwait(false);
#endif
        return (line is not null) ? line.Trim() : line;
    }

    public async Task WriteLineAsync(string? value, CancellationToken cancelToken = default)
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

    public abstract Task ProcessInputAsync(string input, CancellationToken cancelToken = default);

    public virtual Task ProcessCommandAsync(string cmd, IList<string> args)
    {
        switch (cmd)
        {
            default:
                Console.WriteLine($"Command {cmd} not handled");
                break;

            case "clear":
                Console.Clear();
                break;
        }

        return Task.CompletedTask;
    }

    protected virtual Task InitAppAsync(CancellationToken cancelToken) => Task.CompletedTask;

    protected void SubscribeAllEvents<T>(JsonTranslator<T> translator)
    {
        translator.SendingPrompt += this.OnSendingPrompt;
        translator.AttemptingRepair += this.OnAttemptingRepairs;
        translator.CompletionReceived += this.OnCompletionReceived;
    }

    protected virtual void OnException(string input, Exception ex)
    {
        Console.WriteLine("## Could not process request");
        if (ex is TypeChatException tex)
        {
            tex.Print();
        }
        else
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine();
        }
    }

    protected void OnSendingPrompt(Prompt prompt)
    {
        Console.WriteLine("### PROMPT");
        Console.WriteLine(prompt.ToString(true));
        Console.WriteLine("###");
    }

    protected void OnCompletionReceived(string value)
    {
        Console.WriteLine("### COMPLETION");
        Console.WriteLine(value);
        Console.WriteLine("###");
    }

    protected void OnAttemptingRepairs(string value)
    {
        Console.WriteLine("### REPAIRING ERROR:");
        Console.WriteLine(value);
        Console.WriteLine("###");
    }

    protected static void WriteError(Exception ex)
    {
        Console.WriteLine("### EXCEPTION:");
        Console.WriteLine(ex.Message);
        Console.WriteLine("###");
    }

    public static void WriteLines(IEnumerable<string> items)
    {
        foreach (string item in items)
        {
            Console.WriteLine(item);
        }
    }
}
