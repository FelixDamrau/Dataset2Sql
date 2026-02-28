namespace Develix.DataSet2Sql.Importing;

public sealed record DataSetImportRequest(
    string XmlFilePath,
    string DatabaseName,
    DatabaseSettings DatabaseSettings,
    Func<string, bool> ConfirmDropCallback);
