namespace Develix.Dataset2Sql.Importing;

public sealed record DatasetImportRequest(
    string XmlFilePath,
    string DatabaseName,
    DatabaseSettings DatabaseSettings,
    Func<string, bool> ConfirmDropCallback);
