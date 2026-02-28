using Spectre.Console.Cli;

namespace Develix.DataSet2Sql.Cli;

public sealed class ConfigInitCommand : Command<ConfigInitCommandSettings>
{
    public override int Execute(CommandContext context, ConfigInitCommandSettings settings, CancellationToken cancellationToken)
    {
        var configPath = AppConfig.GetConfigPath();
        if (File.Exists(configPath) && !settings.Force)
        {
            Log.Warn($"Configuration file already exists: '{configPath}'.");
            Log.Info("Use '--force' to overwrite it.");
            Log.Info($"Config path: '{configPath}'");
            return 0;
        }

        var configDirectory = Path.GetDirectoryName(configPath);
        if (!string.IsNullOrWhiteSpace(configDirectory))
            Directory.CreateDirectory(configDirectory);

        File.WriteAllText(configPath, AppConfig.CreateDefaultConfigJson());
        Log.Info($"Configuration file created at '{configPath}'.");
        return 0;
    }
}
