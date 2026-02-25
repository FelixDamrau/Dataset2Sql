using System.ComponentModel;
using Spectre.Console.Cli;

namespace Develix.Dataset2Sql;

public sealed class ConfigInitCommandSettings : CommandSettings
{
    [CommandOption("-f|--force")]
    [Description("Overwrite existing settings file.")]
    public bool Force { get; init; }
}
