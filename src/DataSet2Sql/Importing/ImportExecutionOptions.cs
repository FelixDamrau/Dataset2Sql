namespace Develix.DataSet2Sql.Importing;

public sealed record ImportExecutionOptions(
    string? XmlFilePath,
    string? DatabaseName);
