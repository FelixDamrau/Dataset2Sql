using Develix.Dataset2Sql.Importing;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Develix.Dataset2Sql.Cli.Commands;

public sealed class ImportCommand : Command<ImportCommandSettings>
{
    private readonly ImportCommandHandler importCommandHandler = new();

    public override int Execute(CommandContext context, ImportCommandSettings settings, CancellationToken cancellationToken)
    {
        Program.ShowVersionScreen();

        var options = new ImportExecutionOptions(settings.XmlFilePath, settings.DatabaseName, settings.AutoConfirmDrop);
        var callbacks = new ImportExecutionCallbacks(
            Console.IsInputRedirected,
            PromptForXmlPath,
            PromptForDbName,
            ConfirmDatabaseDrop);

        return importCommandHandler.Execute(options, callbacks);
    }

    private static string PromptForXmlPath()
    {
        var prompt = new TextPrompt<string>("[silver]Enter the path to the XML file:[/]")
            .PromptStyle("white")
            .Validate(path =>
                !string.IsNullOrWhiteSpace(path)
                    ? ValidationResult.Success()
                    : ValidationResult.Error("XML file path is required."));

        return AnsiConsole.Prompt(prompt);
    }

    private static string PromptForDbName(string? defaultName)
    {
        var promptMessage = defaultName is null
            ? "[silver]Enter DB Name:[/]"
            : $"[silver]Enter DB Name[/] [[{Markup.Escape(defaultName)}]]";
        var prompt = new TextPrompt<string>(promptMessage)
            .PromptStyle("white")
            .Validate(name =>
                !string.IsNullOrWhiteSpace(name)
                    ? ValidationResult.Success()
                    : ValidationResult.Error("Database name is required."));

        if (!string.IsNullOrWhiteSpace(defaultName))
        {
            prompt = prompt
                .DefaultValue(defaultName)
                .ShowDefaultValue(true);
        }

        return AnsiConsole.Prompt(prompt);
    }

    private static bool ConfirmDatabaseDrop(string dbName, bool autoConfirmDrop)
    {
        if (autoConfirmDrop)
        {
            Log.Info($"'{dbName}' already exists. Dropping automatically because --yes was provided.");
            return true;
        }

        Log.Warn($"Database '{dbName}' already exists.");
        var isConfirmed = DropConfirmationPolicy.ShouldDrop(
            dbName,
            autoConfirmDrop,
            () => AnsiConsole.Prompt(ReadConfirmation(dbName)));

        if (!isConfirmed)
        {
            Log.Info("Database drop not confirmed. Exiting.");
            return false;
        }

        return true;

        static TextPrompt<string> ReadConfirmation(string databaseName)
        {
            return new TextPrompt<string>($"[silver]Type the exact database name to confirm drop[/] [[{Markup.Escape(databaseName)}]]")
                .PromptStyle("white");
        }
    }
}
