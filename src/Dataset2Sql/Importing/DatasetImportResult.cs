namespace Develix.Dataset2Sql.Importing;

public sealed record DatasetImportResult
{
    public required DatasetImportStatus Status { get; init; }
    public string? MissingFilePath { get; init; }
}
