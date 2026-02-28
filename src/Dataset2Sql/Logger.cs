using Spectre.Console;

namespace Develix.Dataset2Sql;

public static class Log
{
    public static void Info(string message) => LogInternal(message, "INFO", "green");
    public static void Warn(string message) => LogInternal(message, "WARN", "yellow");
    public static void Error(string message) => LogInternal(message, "ERROR", "red");

    public static T ProgressBytes<T>(string description, long totalBytes, Func<Action<long>, T> action)
    {
        ArgumentNullException.ThrowIfNull(description);
        ArgumentNullException.ThrowIfNull(action);
        ArgumentOutOfRangeException.ThrowIfNegative(totalBytes);

        if (Console.IsOutputRedirected)
        {
            Info($"{description} (start)");
            var result = action(_ => { });
            Info($"{description} (end)");
            return result;
        }

        var maxValue = Math.Max(totalBytes, 1);
        return AnsiConsole.Progress()
            .Columns([
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new DownloadedColumn(),
                new RemainingTimeColumn(),
                new SpinnerColumn()
            ])
            .Start(context =>
            {
                var task = context.AddTask(Markup.Escape(description), maxValue: maxValue);
                var result = action(bytesRead =>
                {
                    var boundedBytes = Math.Clamp(bytesRead, 0, maxValue);
                    task.Value = boundedBytes;
                });

                task.Value = maxValue;
                return result;
            });
    }

    public static Task<T> StatusAsync<T>(string status, Func<Task<T>> action)
    {
        if (Console.IsOutputRedirected)
        {
            Info(status);
            return action();
        }

        return AnsiConsole.Status().StartAsync(status, _ => action());
    }

    private static void LogInternal(string message, string level, string color)
    {
        var timestamp = DateTimeOffset.Now.ToString("s");
        if (Console.IsOutputRedirected)
        {
            Console.WriteLine($"[{timestamp}] [{level}] {message}");
            return;
        }

        AnsiConsole.MarkupLine($"[grey]{Markup.Escape(timestamp)}[/][{color}] {Markup.Escape(level)}[/]{Markup.Escape(" " + message)}");
    }
}
