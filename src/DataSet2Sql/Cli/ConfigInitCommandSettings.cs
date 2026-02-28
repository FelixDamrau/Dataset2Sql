using System.ComponentModel;
using Spectre.Console.Cli;

namespace Develix.DataSet2Sql.Cli;

public sealed class ConfigInitCommandSettings : CommandSettings
{
    [CommandOption("-f|--force")]
    [Description("Overwrite existing settings file.")]
    public bool Force { get; init; }
}
