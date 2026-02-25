using Spectre.Console.Cli;

namespace Develix.Dataset2Sql.Cli.Commands;

public sealed class ConfigPathCommand : Command
{
    public override int Execute(CommandContext context, CancellationToken cancellationToken)
    {
        Console.WriteLine(AppConfig.GetConfigPath());
        return 0;
    }
}
