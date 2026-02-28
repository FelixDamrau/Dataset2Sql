using System.Text.Json;

namespace Develix.DataSet2Sql;

public static class AppConfig
{
    public const string SettingsFileName = "dataset2sql.settings.json";

    public static string GetConfigPath()
        => Path.Combine(AppContext.BaseDirectory, SettingsFileName);

    public static string GetExecutableCommandName()
    {
        var processPath = Environment.ProcessPath;
        return string.IsNullOrWhiteSpace(processPath)
            ? "DataSet2Sql"
            : Path.GetFileNameWithoutExtension(processPath);
    }

    public static string CreateDefaultConfigJson()
    {
        var settingsFile = new SettingsFileModel { DatabaseSettings = new() };
        return JsonSerializer.Serialize(settingsFile, new JsonSerializerOptions { WriteIndented = true });
    }

    private sealed class SettingsFileModel
    {
        public required DatabaseSettings DatabaseSettings { get; init; }
    }
}
