using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Develix.Dataset2Sql.Importing;

public sealed class ImportCommandHandler
{
    private readonly Func<string> getConfigPath;
    private readonly Func<string> getExecutableCommandName;
    private readonly Func<string, bool> fileExists;
    private readonly Func<string, DatabaseSettings> loadDatabaseSettings;
    private readonly Func<DatasetImportRequest, DatasetImportResult> runImportWorkflow;

    public ImportCommandHandler()
        : this(
            AppConfig.GetConfigPath,
            AppConfig.GetExecutableCommandName,
            File.Exists,
            LoadDatabaseSettings,
            RunImportWorkflow)
    {
    }

    /// <summary>
    /// This method is only used in tests
    /// </summary>
    internal ImportCommandHandler(
        Func<string> getConfigPath,
        Func<string> getExecutableCommandName,
        Func<string, bool> fileExists,
        Func<string, DatabaseSettings> loadDatabaseSettings,
        Func<DatasetImportRequest, DatasetImportResult> runImportWorkflow)
    {
        this.getConfigPath = getConfigPath;
        this.getExecutableCommandName = getExecutableCommandName;
        this.fileExists = fileExists;
        this.loadDatabaseSettings = loadDatabaseSettings;
        this.runImportWorkflow = runImportWorkflow;
    }

    public int Execute(ImportExecutionOptions options, ImportExecutionCallbacks callbacks)
    {
        try
        {
            var configPath = getConfigPath();
            if (!fileExists(configPath))
            {
                Log.Error($"Configuration file not found: '{configPath}'.");
                Log.Info($"Run '{getExecutableCommandName()} config init' to create it next to the executable.");
                Log.Info($"Config path: '{configPath}'");
                return 1;
            }

            var dbSettings = loadDatabaseSettings(configPath);
            var xmlFilePath = XmlPathResolver.Resolve(
                options.XmlFilePath,
                callbacks.IsInputRedirected,
                callbacks.PromptForXmlPath);
            var dbName = DatabaseNameResolver.Resolve(
                options.DatabaseName,
                dbSettings.Name,
                callbacks.IsInputRedirected,
                callbacks.PromptForDatabaseName);

            var request = new DatasetImportRequest(
                xmlFilePath,
                dbName,
                dbSettings,
                databaseName => callbacks.ConfirmDatabaseDrop(databaseName, options.AutoConfirmDrop));
            var result = runImportWorkflow(request);

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

    private static DatabaseSettings LoadDatabaseSettings(string configPath)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(configPath, optional: false)
            .Build();

        var dbSettings = new DatabaseSettings();
        configuration.GetSection("DatabaseSettings").Bind(dbSettings);
        return dbSettings;
    }

    private static DatasetImportResult RunImportWorkflow(DatasetImportRequest request)
    {
        var workflow = new DatasetImportWorkflow(new PhysicalFileSystem(), new XmlDataSetReader(), new DatasetImporterExecutor());
        return workflow.Run(request);
    }
}
