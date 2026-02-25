using Develix.Dataset2Sql.Cli;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Develix.Dataset2Sql;

public class Program
{
    public static int Main(string[] args)
    {
        var routedArgs = args.Length == 0 ? ["import"] : args;

        var app = new CommandApp();
        app.Configure(config =>
        {
            config.SetApplicationName("Dataset2Sql");
            config.Settings.StrictParsing = true;
            config.AddCommand<ImportCommand>("import");
            config.AddBranch("config", branch =>
            {
                branch.AddCommand<ConfigInitCommand>("init");
                branch.AddCommand<ConfigPathCommand>("path");
            });
            config.ValidateExamples();
            config.AddExample();
            config.AddExample(["import", "--xml", "./dump.xml", "--db", "DumpDb", "--yes"]);
            config.AddExample(["config", "path"]);
            config.AddExample(["config", "init"]);
        });

        var exitCode = app.Run(routedArgs);

        if (ShouldPauseOnExit(args, Console.IsInputRedirected, Console.IsOutputRedirected))
        {
            AnsiConsole.MarkupLine("[grey]Press any key to exit...[/]");
            Console.ReadKey();
        }

        return exitCode;
    }

    internal static bool ShouldPauseOnExit(string[] args, bool isInputRedirected, bool isOutputRedirected)
        => args.Length == 0 && !isInputRedirected && !isOutputRedirected;
}
