using Spectre.Console.Cli;

namespace Develix.DataSet2Sql.Cli;

public sealed class ConfigPathCommand : Command
{
    public override int Execute(CommandContext context, CancellationToken cancellationToken)
    {
        Console.WriteLine(AppConfig.GetConfigPath());
        return 0;
    }
}
