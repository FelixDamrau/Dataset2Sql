using System.Reflection;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Develix.Dataset2Sql;

public class Program
{
    public static int Main(string[] args)
    {
        ShowVersionScreen();

        var app = new CommandApp<ImportCommand>();
        app.Configure(config =>
        {
            config.SetApplicationName("Dataset2Sql");
            config.Settings.StrictParsing = true;
            config.ValidateExamples();
            config.AddExample(["--xml", "./dump.xml", "--db", "DumpDb", "--yes"]);
        });

        var exitCode = app.Run(args);

        if (ShouldPauseOnExit(args, Console.IsInputRedirected, Console.IsOutputRedirected))
        {
            AnsiConsole.MarkupLine("[grey]Press any key to exit...[/]");
            Console.ReadKey();
        }

        return exitCode;
    }

    internal static bool ShouldPauseOnExit(string[] args, bool isInputRedirected, bool isOutputRedirected)
        => args.Length == 0 && !isInputRedirected && !isOutputRedirected;

    private static void ShowVersionScreen()
    {
        var version = Assembly.GetExecutingAssembly()!.GetName().Version;
        var versionText = $"Dataset2Sql v{version}";

        if (Console.IsOutputRedirected)
        {
            Console.WriteLine(versionText);
            return;
        }

        var panel = new Panel($"[bold]{Markup.Escape(versionText)}[/]")
        {
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Grey),
            Padding = new Padding(1, 0, 1, 0)
        };

        AnsiConsole.Write(panel);
    }
}
