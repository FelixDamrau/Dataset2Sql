using System.ComponentModel;
using Spectre.Console.Cli;

namespace Develix.Dataset2Sql;

public sealed class ImportCommandSettings : CommandSettings
{
    [CommandOption("-x|--xml <XML_PATH>")]
    [Description("XML file to import.")]
    public string? XmlFilePath { get; init; }

    [CommandOption("-d|--db <DB_NAME>")]
    [Description("Database name override.")]
    public string? DatabaseName { get; init; }

    [CommandOption("-y|--yes")]
    [Description("Automatically confirm database drop.")]
    public bool AutoConfirmDrop { get; init; }
}
