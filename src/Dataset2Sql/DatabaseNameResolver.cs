namespace Develix.Dataset2Sql;

public static class DatabaseNameResolver
{
    public static string Resolve(
        string? databaseName,
        string defaultDatabaseName,
        bool isInputRedirected,
        Func<string?, string> promptForDbName)
    {
        if (!string.IsNullOrWhiteSpace(databaseName))
            return databaseName;

        if (isInputRedirected)
        {
            return !string.IsNullOrWhiteSpace(defaultDatabaseName)
                ? defaultDatabaseName
                : throw new InvalidOperationException("Database name is required in non-interactive mode. Use --db.");
        }

        return promptForDbName(string.IsNullOrWhiteSpace(defaultDatabaseName) ? null : defaultDatabaseName);
    }
}
