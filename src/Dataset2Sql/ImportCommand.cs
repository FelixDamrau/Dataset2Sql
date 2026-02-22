using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Spectre.Console;
using Spectre.Console.Cli;
using Develix.Dataset2Sql.Importing;

namespace Develix.Dataset2Sql;

public sealed class ImportCommand : Command<ImportCommandSettings>
{
    public override int Execute(CommandContext context, ImportCommandSettings settings, CancellationToken cancellationToken)
    {
        try
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var dbSettings = new DatabaseSettings();
            configuration.GetSection("DatabaseSettings").Bind(dbSettings);

            var xmlFilePath = ResolveXmlPath(settings);
            var dbName = ResolveDatabaseName(settings, dbSettings.Name);

            var workflow = new DatasetImportWorkflow(new PhysicalFileSystem(), new XmlDataSetReader(), new DatasetImporterExecutor());
            var request = new DatasetImportRequest(
                xmlFilePath,
                dbName,
                dbSettings,
                name => ConfirmDatabaseDrop(name, settings.AutoConfirmDrop));
            var result = workflow.Run(request);

            if (result.Status == DatasetImportStatus.FileNotFound)
            {
                Log.Error($"File not found at '{result.MissingFilePath}'.");
                return 1;
            }

            if (result.Status == DatasetImportStatus.Cancelled)
            {
                Log.Info("Import canceled by user.");
                return 0;
            }

            Log.Info("Import completed successfully.");
            return 0;
        }
        catch (SqlException ex)
        {
            Log.Error($"SQL error ({ex.Number}): {ex.Message}");
            if (ex.InnerException != null)
                Log.Error($"Inner exception: {ex.InnerException.Message}");
            return 1;
        }
        catch (IOException ex)
        {
            Log.Error($"I/O error: {ex.Message}");
            return 1;
        }
        catch (Exception ex)
        {
            Log.Error(ex.Message);
            if (ex.InnerException != null)
                Log.Error($"Inner exception: {ex.InnerException.Message}");
            return 1;
        }
    }

    private static string ResolveXmlPath(ImportCommandSettings settings)
    {
        var promptForXmlPath = new TextPrompt<string>("[silver]Enter the path to the XML file:[/]")
            .PromptStyle("white")
            .Validate(path =>
                !string.IsNullOrWhiteSpace(path)
                    ? ValidationResult.Success()
                    : ValidationResult.Error("XML file path is required."));
        return XmlPathResolver.Resolve(
            settings.XmlFilePath,
            Console.IsInputRedirected,
            () => AnsiConsole.Prompt(promptForXmlPath));
    }

    private static string ResolveDatabaseName(ImportCommandSettings settings, string defaultDatabaseName)
    {
        return DatabaseNameResolver.Resolve(
            settings.DatabaseName,
            defaultDatabaseName,
            Console.IsInputRedirected,
            PromptForDbName
        );

        static string PromptForDbName(string? defaultName)
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

        static TextPrompt<string> ReadConfirmation(string dbName)
        {
            return new TextPrompt<string>($"[silver]Type the exact database name to confirm drop[/] [[{Markup.Escape(dbName)}]]")
                .PromptStyle("white");
        }
    }
}
