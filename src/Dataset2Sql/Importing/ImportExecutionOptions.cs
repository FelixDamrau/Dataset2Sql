namespace Develix.Dataset2Sql.Importing;

public sealed record ImportExecutionOptions(
    string? XmlFilePath,
    string? DatabaseName);
