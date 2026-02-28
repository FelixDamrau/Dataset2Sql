namespace Develix.DataSet2Sql.Importing;

public sealed record DataSetImportResult
{
    public required DataSetImportStatus Status { get; init; }
    public string? MissingFilePath { get; init; }
}
