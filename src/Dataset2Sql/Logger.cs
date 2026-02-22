using Spectre.Console;

namespace Develix.Dataset2Sql;

public static class Log
{
    public static void Info(string message) => LogInternal(message, "INFO", "green");
    public static void Warn(string message) => LogInternal(message, "WARN", "yellow");
    public static void Error(string message) => LogInternal(message, "ERROR", "red");

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
