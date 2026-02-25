namespace Develix.Dataset2Sql.Importing;

public sealed record ImportExecutionCallbacks(
    bool IsInputRedirected,
    Func<string> PromptForXmlPath,
    Func<string?, string> PromptForDatabaseName,
    Func<string, bool, bool> ConfirmDatabaseDrop);
