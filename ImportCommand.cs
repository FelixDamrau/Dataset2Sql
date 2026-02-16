using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Develix.Dataset2Sql;

internal sealed class ImportCommand : Command<ImportCommandSettings>
{
    public override int Execute(CommandContext context, ImportCommandSettings settings, CancellationToken cancellationToken)
    {
        try
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var dbSettings = new DatabaseSettings();
            configuration.GetSection("DatabaseSettings").Bind(dbSettings);

            var xmlFilePath = ResolveXmlPath(settings);
            if (!File.Exists(xmlFilePath))
            {
                Log.Error($"File not found at '{xmlFilePath}'.");
                return 1;
            }

            var dbName = ResolveDatabaseName(settings, dbSettings.Name);

            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = dbSettings.Server,
                InitialCatalog = dbName,
                TrustServerCertificate = true,
                UserID = dbSettings.Username,
                Password = dbSettings.Password
            };

            var dataSet = new DataSet();
            dataSet.ReadXml(xmlFilePath);

            using var commandBuilder = new SqlCommandBuilder();
            var importer = new DatasetImporter(commandBuilder);

            importer.ImportDatasetToSqlServer(dataSet, connectionStringBuilder, dbName, name => ConfirmDatabaseDrop(name, settings.AutoConfirmDrop));

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
        finally
        {
            if (settings.IsInteractive)
            {
                AnsiConsole.MarkupLine("[grey]Press any key to exit...[/]");
                Console.ReadKey();
            }
        }
    }

    private static string ResolveXmlPath(ImportCommandSettings settings)
    {
        if (!string.IsNullOrWhiteSpace(settings.XmlFilePath))
            return settings.XmlFilePath.Trim('"', '\'');

        if (Console.IsInputRedirected)
            throw new InvalidOperationException("XML file path is required in non-interactive mode. Use --xml.");

        return AnsiConsole.Prompt(
            new TextPrompt<string>("[silver]Enter the path to the XML file:[/]")
                .PromptStyle("white")
                .Validate(path =>
                    !string.IsNullOrWhiteSpace(path)
                        ? ValidationResult.Success()
                        : ValidationResult.Error("XML file path is required."))
        ).Trim('"', '\'');
    }

    private static string ResolveDatabaseName(ImportCommandSettings settings, string defaultDatabaseName)
    {
        if (!string.IsNullOrWhiteSpace(settings.DatabaseName))
            return settings.DatabaseName;

        if (Console.IsInputRedirected)
        {
            return !string.IsNullOrWhiteSpace(defaultDatabaseName)
                ? defaultDatabaseName
                : throw new InvalidOperationException("Database name is required in non-interactive mode. Use --db.");
        }

        if (string.IsNullOrWhiteSpace(defaultDatabaseName))
        {
            return AnsiConsole.Prompt(
                new TextPrompt<string>("[silver]Enter DB Name:[/]")
                    .PromptStyle("white")
                    .Validate(name =>
                        !string.IsNullOrWhiteSpace(name)
                            ? ValidationResult.Success()
                            : ValidationResult.Error("Database name is required."))
            );
        }

        return AnsiConsole.Prompt(
            new TextPrompt<string>($"[silver]Enter DB Name[/] [[{Markup.Escape(defaultDatabaseName)}]]")
                .PromptStyle("white")
                .DefaultValue(defaultDatabaseName)
                .ShowDefaultValue(true)
        );
    }

    private static bool ConfirmDatabaseDrop(string dbName, bool autoConfirmDrop)
    {
        if (autoConfirmDrop)
        {
            Log.Warn($"'{dbName}' already exists. Dropping automatically because --yes was provided.");
            return true;
        }

        Log.Warn($"Database '{Markup.Escape(dbName)}' already exists.");
        var response = AnsiConsole.Prompt(
            new TextPrompt<string>($"[silver]Type the exact database name to confirm drop[/] [[{Markup.Escape(dbName)}]]")
                .PromptStyle("white")
        );

        if (!string.Equals(response, dbName, StringComparison.Ordinal))
        {
            Log.Info("Database drop not confirmed. Exiting.");
            Environment.Exit(0);
            return false;
        }

        return true;
    }
}
